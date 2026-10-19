using Electium.Application.Constants;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Web.Extensions;
using Electium.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

[Authorize(Roles = AppRoles.AdminOrLeader)]
public class ResultsController : Controller
{
    private readonly IElectionService _elections;
    private readonly IVotingService _voting;
    private readonly IPartyService _parties;

    public ResultsController(IElectionService elections, IVotingService voting, IPartyService parties)
    {
        _elections = elections;
        _voting = voting;
        _parties = parties;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? electionId)
    {
        var partyId = User.GetPartyId();
        var elections = await _elections.GetAllAsync();

        var selected = electionId
            ?? (await _elections.GetOpenAsync())?.Id
            ?? elections.FirstOrDefault()?.Id;

        var model = new ResultsViewModel
        {
            Elections = elections,
            SelectedElectionId = selected,
            IsLeader = partyId is not null,
            PartyName = partyId is null
                ? null
                : (await _parties.GetAllAsync()).FirstOrDefault(p => p.Id == partyId)?.Name
        };

        if (selected is not null)
        {
            try
            {
                model.Results = await _voting.GetResultsAsync(selected.Value, partyId);
            }
            catch (BusinessException ex)
            {
                TempData["Error"] = ex.Message;
            }
        }

        return View(model);
    }
}
