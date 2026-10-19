using Electium.Application.Constants;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class LeadersController : Controller
{
    private readonly IAccountService _account;
    private readonly IPartyService _parties;

    public LeadersController(IAccountService account, IPartyService parties)
    {
        _account = account;
        _parties = parties;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
        => View(await _account.GetLeadersAsync());

    [HttpGet]
    public async Task<IActionResult> Create()
        => View(await FillPartiesAsync(new LeaderViewModel()));

    [HttpPost]
    public async Task<IActionResult> Create(LeaderViewModel model)
    {
        if (!ModelState.IsValid)
            return View(await FillPartiesAsync(model));

        try
        {
            await _account.CreateLeaderAsync(model);
            TempData["Success"] = "El dirigente político se registró correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await FillPartiesAsync(model));
        }
    }

    [HttpPost]
    public async Task<IActionResult> SetActive(string id, bool isActive)
    {
        try
        {
            await _account.SetLeaderActiveAsync(id, isActive);
            TempData["Success"] = isActive ? "Cuenta activada." : "Cuenta desactivada.";
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _account.DeleteLeaderAsync(id);
            TempData["Success"] = "El dirigente se eliminó correctamente.";
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<LeaderViewModel> FillPartiesAsync(LeaderViewModel model)
    {
        model.Parties = (await _parties.GetAllAsync())
            .Where(p => p.IsActive)
            .Select(p => new LookupItem(p.Id, p.Name))
            .ToList();

        return model;
    }
}
