using Electium.Application.Constants;
using Electium.Application.Interfaces;
using Electium.Web.Extensions;
using Electium.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

[Authorize(Roles = AppRoles.AdminOrLeader)]
public class DashboardController : Controller
{
    private readonly ICitizenService _citizens;
    private readonly IPartyService _parties;
    private readonly IElectivePositionService _positions;
    private readonly IElectionService _elections;
    private readonly ICandidateService _candidates;

    public DashboardController(
        ICitizenService citizens,
        IPartyService parties,
        IElectivePositionService positions,
        IElectionService elections,
        ICandidateService candidates)
    {
        _citizens = citizens;
        _parties = parties;
        _positions = positions;
        _elections = elections;
        _candidates = candidates;
    }

    public async Task<IActionResult> Index()
    {
        var partyId = User.GetPartyId();
        var allParties = await _parties.GetAllAsync();

        var model = new DashboardViewModel
        {
            IsLeader = partyId is not null,
            PartyName = allParties.FirstOrDefault(p => p.Id == partyId)?.Name,
            Citizens = (await _citizens.GetAllAsync()).Count,
            Parties = allParties.Count,
            Positions = (await _positions.GetAllAsync()).Count,
            Elections = (await _elections.GetAllAsync()).Count,
            Candidates = (await _candidates.GetFilteredAsync(partyId: partyId)).Count,
            OpenElection = await _elections.GetOpenAsync()
        };

        return View(model);
    }
}
