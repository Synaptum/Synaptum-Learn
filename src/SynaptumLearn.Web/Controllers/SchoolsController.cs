using MediatR;
using Microsoft.AspNetCore.Mvc;
using SynaptumLearn.Web.ViewModels.Schools;
using SynaptumLearn.Application.Schools.CreateSchool;
using Microsoft.Identity.Client;


namespace SynaptumLearn.Web.Controllers;

public class SchoolsController : Controller
{
    private readonly IMediator _mediator;

    public SchoolsController(IMediator mediator)
    {
        _mediator = mediator;
    }

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

    [HttpGet]
    public IActionResult Index()
    {
        return View();  
    }
}