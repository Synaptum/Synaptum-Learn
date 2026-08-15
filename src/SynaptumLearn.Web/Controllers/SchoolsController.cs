using MediatR;
using Microsoft.AspNetCore.Mvc;
using SynaptumLearn.Web.ViewModels.Schools;
using SynaptumLearn.Application.Schools.CreateSchool;
using SynaptumLearn.Application.Schools.ListSchools;
using SynaptumLearn.Application.Schools.GetSchoolDetails;
using SynaptumLearn.Application.Schools.UpdateSchool;
using SynaptumLearn.Application.Schools.ChangeSchoolStatus;
using SynaptumLearn.Domain.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using SynaptumLearn.Web.Models.Schools;



namespace SynaptumLearn.Web.Controllers;

public class SchoolsController : Controller
{
    private readonly IMediator _mediator;

    public SchoolsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateSchoolViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSchoolViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var command = new CreateSchoolCommand
        {
            Name = model.Name,
            EMISNumber = model.EMISNumber,
            Email = model.Email,
            Phone = model.Phone,
            Province = model.Province
        };

        var id = await _mediator.Send(command);

        return RedirectToAction(nameof(Index));
    }
    #endregion
    #region Index
    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var schools = await _mediator.Send(
            new ListSchoolsQuery(),
            cancellationToken);

        return View(schools);
    }
    #endregion
    #region Details
    [HttpGet]
    public async Task<IActionResult> Details(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var school = await _mediator.Send(
            new GetSchoolDetailsQuery(id),
            cancellationToken
        );

        if (school is null)
        {
            return NotFound();
        }

        return View(school);
    }
    #endregion
    #region Edit

    [HttpGet]
    public async Task<IActionResult> Edit(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var school = await _mediator.Send(
            new GetSchoolDetailsQuery(id),
            cancellationToken
        );

        if (school is null)
        {
            return NotFound();
        }

        var model = new EditSchoolViewModel
        {
            Id = school.Id,
            Name = school.Name,
            EMISNumber = school.EMISNumber,
            Email = school.Email,
            Phone = school.Phone,
            Province = school.Province
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        EditSchoolViewModel model,
        CancellationToken cancellationToken
    )
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var command = new UpdateSchoolCommand(
            model.Id,
            model.Name,
            model.EMISNumber,
            model.Email,
            model.Phone,
            model.Province
        );

        var updated = await _mediator.Send(
            command,
            cancellationToken
        );

        if (!updated)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "School updated successfully.";

        return RedirectToAction(
            nameof(Details),
            new { id = model.Id }
        );
    }
    #endregion
    #region ChangeStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        SchoolStatus newStatus,
        CancellationToken cancellationToken
    )
    {
        var changed = await _mediator.Send(
            new ChangeSchoolStatusCommand(id, newStatus),
            cancellationToken
        );

        if (!changed)
        {
            return BadRequest();
        }

        TempData["SuccessMessage"] = "School Status updated successfully.";

        return RedirectToAction(
            nameof(Details),
            new { id }
        );
    }
    #endregion

}