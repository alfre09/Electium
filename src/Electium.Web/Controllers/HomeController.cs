using System.Diagnostics;
using Electium.Application.Constants;
using Electium.Application.Dtos;
using Electium.Application.Interfaces;
using Electium.Domain.Enums;
using Electium.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

public class HomeController : Controller
{
    private readonly IElectionService _elections;
    private readonly ICandidateService _candidates;
    private readonly IVotingService _voting;

    public HomeController(IElectionService elections, ICandidateService candidates, IVotingService voting)
    {
        _elections = elections;
        _candidates = candidates;
        _voting = voting;
    }

    public async Task<IActionResult> Index(int? year)
    {
        var all = await _elections.GetAllAsync();
        var open = all.FirstOrDefault(e => e.Status == ElectionStatus.Open);

        var model = new HomeViewModel
        {
            IsAdmin = User.IsInRole(AppRoles.Admin),
            IsLeader = User.IsInRole(AppRoles.Leader),
            HasActiveElection = open is not null,
            ActiveElectionName = open?.Name
        };

        // El resumen por año es información administrativa: solo para admin y dirigentes.
        if (model.IsStaff)
        {
            // Año de la elección: cuando se abrió, o cuando se creó si aún no se abre.
            int YearOf(ElectionDto e) => (e.OpenedAt ?? e.CreatedAt).Year;

            model.ElectionYears = all.Select(YearOf).Distinct().OrderByDescending(y => y).ToList();
            model.SelectedYear = year ?? model.ElectionYears.FirstOrDefault();

            if (model.ElectionYears.Count > 0 && model.SelectedYear is not null)
            {
                foreach (var election in all.Where(e => YearOf(e) == model.SelectedYear))
                {
                    var candidates = await _candidates.GetFilteredAsync(election.Id);
                    var results = await _voting.GetResultsAsync(election.Id);

                    model.Summaries.Add(new ElectionSummaryViewModel
                    {
                        Name = election.Name,
                        Status = election.Status,
                        PartyCount = candidates.Select(c => c.PartyId).Distinct().Count(),
                        CandidateCount = candidates.Count,
                        TotalVotes = results.Positions.Sum(p => p.TotalVotes)
                    });
                }
            }
        }

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        ViewBag.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View();
    }
}
