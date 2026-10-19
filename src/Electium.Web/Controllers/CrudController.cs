using AutoMapper;
using Electium.Application.Dtos;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Application.ViewModels;
using Electium.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

/// <summary>
/// CRUD base: Index, Create, Edit y Delete con ViewModels + AutoMapper.
/// Las vistas (Index.cshtml y Form.cshtml) viven en la carpeta de cada controlador.
/// </summary>
public abstract class CrudController<TViewModel, TDto, TEntity> : Controller
    where TViewModel : BaseViewModel, new()
    where TDto : BaseDto
    where TEntity : BaseEntity
{
    protected readonly IGenericService<TDto, TEntity> Service;
    protected readonly IMapper Mapper;

    protected CrudController(IGenericService<TDto, TEntity> service, IMapper mapper)
    {
        Service = service;
        Mapper = mapper;
    }

    /// <summary>Texto para los mensajes, por ejemplo "El ciudadano".</summary>
    protected abstract string EntityLabel { get; }

    [HttpGet]
    public virtual async Task<IActionResult> Index()
    {
        var items = await Service.GetAllAsync();
        return View(Mapper.Map<List<TViewModel>>(items));
    }

    [HttpGet]
    public virtual IActionResult Create() => View("Form", new TViewModel());

    [HttpPost]
    public virtual async Task<IActionResult> Create(TViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Form", model);

        try
        {
            await Service.AddAsync(Mapper.Map<TDto>(model));
            TempData["Success"] = $"{EntityLabel} se registró correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Form", model);
        }
    }

    [HttpGet]
    public virtual async Task<IActionResult> Edit(int id)
    {
        var dto = await Service.GetByIdAsync(id);
        if (dto is null)
            return NotFound();

        return View("Form", Mapper.Map<TViewModel>(dto));
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit(int id, TViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View("Form", model);

        try
        {
            await Service.UpdateAsync(id, Mapper.Map<TDto>(model));
            TempData["Success"] = $"{EntityLabel} se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Form", model);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> Delete(int id)
    {
        try
        {
            await Service.DeleteAsync(id);
            TempData["Success"] = $"{EntityLabel} se eliminó correctamente.";
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
