using AutoMapper;
using Electium.Application.Constants;
using Electium.Application.Dtos;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Application.ViewModels;
using Electium.Domain.Enums;
using Electium.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

/// <summary>
/// El administrador gestiona los candidatos de todos los partidos.
/// El dirigente político solo ve y gestiona los de su propio partido.
/// </summary>
[Authorize(Roles = AppRoles.AdminOrLeader)]
public class CandidatesController : Controller
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxPhotoBytes = 2 * 1024 * 1024;

    private readonly ICandidateService _candidates;
    private readonly IElectionService _elections;
    private readonly IPartyService _parties;
    private readonly IElectivePositionService _positions;
    private readonly ICitizenService _citizens;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;

    public CandidatesController(
        ICandidateService candidates,
        IElectionService elections,
        IPartyService parties,
        IElectivePositionService positions,
        ICitizenService citizens,
        IMapper mapper,
        IWebHostEnvironment env)
    {
        _candidates = candidates;
        _elections = elections;
        _parties = parties;
        _positions = positions;
        _citizens = citizens;
        _mapper = mapper;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? electionId)
    {
        ViewBag.Elections = await _elections.GetAllAsync();
        ViewBag.SelectedElectionId = electionId;
        ViewBag.IsLeader = User.GetPartyId() is not null;

        var list = await _candidates.GetFilteredAsync(electionId, User.GetPartyId());
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = await BuildFormAsync(new CandidateViewModel());
        return View("Form", model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CandidateViewModel model, IFormFile? photo)
    {
        ApplyLeaderScope(model);

        if (!ModelState.IsValid)
            return View("Form", await BuildFormAsync(model));

        string? photoPath = null;
        try
        {
            photoPath = await SavePhotoAsync(photo);

            var dto = _mapper.Map<CandidateDto>(model);
            dto.PhotoPath = photoPath;

            await _candidates.AddAsync(dto);

            TempData["Success"] = "El candidato se registró correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            DeletePhoto(photoPath);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Form", await BuildFormAsync(model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var existing = await _candidates.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        if (!CanManage(existing))
            return Forbid();

        var model = _mapper.Map<CandidateViewModel>(existing);
        return View("Form", await BuildFormAsync(model));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, CandidateViewModel model, IFormFile? photo)
    {
        if (id != model.Id)
            return BadRequest();

        var existing = await _candidates.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        if (!CanManage(existing))
            return Forbid();

        ApplyLeaderScope(model);
        model.PhotoPath = existing.PhotoPath; // nunca se confía en la ruta que llega del formulario

        if (!ModelState.IsValid)
            return View("Form", await BuildFormAsync(model));

        string? newPhotoPath = null;
        try
        {
            newPhotoPath = await SavePhotoAsync(photo);

            var dto = _mapper.Map<CandidateDto>(model);
            dto.PhotoPath = newPhotoPath ?? existing.PhotoPath;

            await _candidates.UpdateAsync(id, dto);

            if (newPhotoPath is not null)
                DeletePhoto(existing.PhotoPath);

            TempData["Success"] = "El candidato se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            DeletePhoto(newPhotoPath);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Form", await BuildFormAsync(model));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _candidates.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        if (!CanManage(existing))
            return Forbid();

        try
        {
            await _candidates.DeleteAsync(id);
            DeletePhoto(existing.PhotoPath);
            TempData["Success"] = "El candidato se eliminó correctamente.";
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // ---------- Ayudantes ----------

    private bool CanManage(CandidateDto candidate)
    {
        var partyId = User.GetPartyId();
        return partyId is null || candidate.PartyId == partyId;
    }

    /// <summary>Si es dirigente, el partido siempre es el suyo, sin importar lo que llegue del formulario.</summary>
    private void ApplyLeaderScope(CandidateViewModel model)
    {
        var partyId = User.GetPartyId();
        if (partyId is not null)
        {
            model.PartyId = partyId.Value;
            ModelState.Remove(nameof(CandidateViewModel.PartyId));
        }
    }

    private async Task<CandidateViewModel> BuildFormAsync(CandidateViewModel model)
    {
        var partyId = User.GetPartyId();

        var elections = await _elections.GetAllAsync();
        model.Elections = elections
            .Where(e => e.Status == ElectionStatus.Draft)
            .Select(e => new LookupItem(e.Id, e.Name))
            .ToList();

        var parties = (await _parties.GetAllAsync()).Where(p => p.IsActive);
        if (partyId is not null)
        {
            parties = parties.Where(p => p.Id == partyId);
            model.PartyId = partyId.Value;
            model.PartyLocked = true;
        }
        model.Parties = parties.Select(p => new LookupItem(p.Id, p.Name)).ToList();

        model.Positions = (await _positions.GetAllAsync())
            .Where(p => p.IsActive)
            .Select(p => new LookupItem(p.Id, p.Name))
            .ToList();

        model.Citizens = (await _citizens.GetAllAsync())
            .Where(c => c.IsActive)
            .Select(c => new LookupItem(c.Id, $"{c.FullName} ({c.IdentityDocument})"))
            .ToList();

        return model;
    }

    private async Task<string?> SavePhotoAsync(IFormFile? photo)
    {
        if (photo is null || photo.Length == 0)
            return null;

        if (photo.Length > MaxPhotoBytes)
            throw new BusinessException("La foto no debe superar los 2 MB.");

        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new BusinessException("La foto debe ser JPG, PNG o WEBP.");

        var folder = Path.Combine(_env.WebRootPath, "uploads", "candidates");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        await using var stream = System.IO.File.Create(Path.Combine(folder, fileName));
        await photo.CopyToAsync(stream);

        return $"/uploads/candidates/{fileName}";
    }

    private void DeletePhoto(string? photoPath)
    {
        if (string.IsNullOrWhiteSpace(photoPath) || !photoPath.StartsWith("/uploads/candidates/"))
            return;

        var fileName = Path.GetFileName(photoPath);
        var fullPath = Path.Combine(_env.WebRootPath, "uploads", "candidates", fileName);

        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);
    }
}
