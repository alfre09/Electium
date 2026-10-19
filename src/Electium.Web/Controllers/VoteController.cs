using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

/// <summary>Flujo público del elector (sin inicio de sesión).</summary>
public class VoteController : Controller
{
    private const string SessionCitizenId = "Voter.CitizenId";
    private const string SessionElectionId = "Voter.ElectionId";
    private const string SessionVoterName = "Voter.Name";
    private const long MaxImageBytes = 5 * 1024 * 1024;

    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png" };

    private readonly IVotingService _voting;
    private readonly IElectionService _elections;
    private readonly IOcrService _ocr;
    private readonly ILogger<VoteController> _logger;

    public VoteController(
        IVotingService voting,
        IElectionService elections,
        IOcrService ocr,
        ILogger<VoteController> logger)
    {
        _voting = voting;
        _elections = elections;
        _ocr = ocr;
        _logger = logger;
    }

    // Paso 1: documento + foto
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        HttpContext.Session.Clear();
        return View(await NewLoginModelAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Index(VoterLoginViewModel model)
    {
        model.ElectionName = (await _elections.GetOpenAsync())?.Name;
        model.OcrEnabled = _ocr.IsEnabled;

        if (_ocr.IsEnabled)
        {
            if (model.IdentityImage is null || model.IdentityImage.Length == 0)
                ModelState.AddModelError(nameof(model.IdentityImage), "Adjunte una foto de su documento.");
            else if (model.IdentityImage.Length > MaxImageBytes)
                ModelState.AddModelError(nameof(model.IdentityImage), "La imagen no debe superar los 5 MB.");
            else if (!AllowedContentTypes.Contains(model.IdentityImage.ContentType))
                ModelState.AddModelError(nameof(model.IdentityImage), "La imagen debe ser JPG o PNG.");
        }

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var voter = await _voting.ValidateVoterAsync(model.IdentityDocument);

            if (_ocr.IsEnabled)
            {
                await using var stream = model.IdentityImage!.OpenReadStream();
                var matches = await _ocr.DocumentMatchesAsync(stream, model.IdentityDocument);

                if (!matches)
                {
                    ModelState.AddModelError(string.Empty,
                        "No pudimos verificar el número en la foto. Use una imagen nítida, con buena luz y el documento completo.");
                    return View(model);
                }
            }

            HttpContext.Session.SetInt32(SessionCitizenId, voter.CitizenId);
            HttpContext.Session.SetInt32(SessionElectionId, voter.ElectionId);
            HttpContext.Session.SetString(SessionVoterName, voter.CitizenFullName);

            return RedirectToAction(nameof(Ballot));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fallo del OCR durante la validación del elector.");
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    // Paso 2: boleta
    [HttpGet]
    public async Task<IActionResult> Ballot()
    {
        if (!TryGetVoter(out _, out var electionId))
            return RedirectToAction(nameof(Index));

        try
        {
            ViewBag.VoterName = HttpContext.Session.GetString(SessionVoterName);
            return View(await _voting.GetBallotAsync(electionId));
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Ballot(Dictionary<int, int>? selections)
    {
        if (!TryGetVoter(out var citizenId, out var electionId))
            return RedirectToAction(nameof(Index));

        try
        {
            await _voting.CastVotesAsync(citizenId, electionId, selections ?? new Dictionary<int, int>());

            TempData["VoterName"] = HttpContext.Session.GetString(SessionVoterName);
            HttpContext.Session.Clear();

            return RedirectToAction(nameof(Done));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.VoterName = HttpContext.Session.GetString(SessionVoterName);

            try
            {
                return View(await _voting.GetBallotAsync(electionId));
            }
            catch (BusinessException)
            {
                return RedirectToAction(nameof(Index));
            }
        }
    }

    // Paso 3: confirmación
    [HttpGet]
    public IActionResult Done()
    {
        ViewBag.VoterName = TempData["VoterName"] as string;
        return View();
    }

    // ---------- Ayudantes ----------

    private bool TryGetVoter(out int citizenId, out int electionId)
    {
        var citizen = HttpContext.Session.GetInt32(SessionCitizenId);
        var election = HttpContext.Session.GetInt32(SessionElectionId);

        citizenId = citizen ?? 0;
        electionId = election ?? 0;
        return citizen is not null && election is not null;
    }

    private async Task<VoterLoginViewModel> NewLoginModelAsync()
        => new()
        {
            ElectionName = (await _elections.GetOpenAsync())?.Name,
            OcrEnabled = _ocr.IsEnabled
        };
}
