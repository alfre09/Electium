using AutoMapper;
using Electium.Application.Constants;
using Electium.Application.Dtos;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Application.ViewModels;
using Electium.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class CitizensController : CrudController<CitizenViewModel, CitizenDto, Citizen>
{
    public CitizensController(ICitizenService service, IMapper mapper) : base(service, mapper)
    {
    }

    protected override string EntityLabel => "El ciudadano";
}

[Authorize(Roles = AppRoles.Admin)]
public class PartiesController : CrudController<PartyViewModel, PartyDto, Party>
{
    public PartiesController(IPartyService service, IMapper mapper) : base(service, mapper)
    {
    }

    protected override string EntityLabel => "El partido";
}

[Authorize(Roles = AppRoles.Admin)]
public class PositionsController : CrudController<ElectivePositionViewModel, ElectivePositionDto, ElectivePosition>
{
    public PositionsController(IElectivePositionService service, IMapper mapper) : base(service, mapper)
    {
    }

    protected override string EntityLabel => "El puesto electivo";
}

[Authorize(Roles = AppRoles.Admin)]
public class ElectionsController : CrudController<ElectionViewModel, ElectionDto, Election>
{
    private readonly IElectionService _elections;

    public ElectionsController(IElectionService service, IMapper mapper) : base(service, mapper)
    {
        _elections = service;
    }

    protected override string EntityLabel => "La elección";

    [HttpPost]
    public async Task<IActionResult> Open(int id)
    {
        try
        {
            await _elections.OpenAsync(id);
            TempData["Success"] = "La elección fue abierta. Ya se puede votar.";
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Close(int id)
    {
        try
        {
            await _elections.CloseAsync(id);
            TempData["Success"] = "La elección fue cerrada. Ya puede consultar los resultados finales.";
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
