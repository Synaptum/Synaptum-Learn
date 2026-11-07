
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SynaptumLearn.Application.Users.CreateUser;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Application.Schools.ListSchools;
using SynaptumLearn.Application.Users.ListManagedUsers;

namespace SynaptumLearn.Web.Controllers;

[Authorize(Roles = PlatformRoles.PlatformAdmin)]
public class UsersController : Controller
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    #region Index
    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var users = await _sender.Send(
            new ListManagedUsersQuery(),
            cancellationToken);

        return View(users);
    }
    #endregion

    #region Create
    [HttpGet]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        await LoadSchoolsAsync(cancellationToken);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadSchoolsAsync(cancellationToken);
            return View(command);
        }

        try
        {
            await _sender.Send(command, cancellationToken);

            TempData["SuccessMessage"] =
                "User created successfully.";

            return RedirectToAction(nameof(Create));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadSchoolsAsync(cancellationToken);
            return View(command);
        }
    }
    #endregion

    #region Private method
    private async Task LoadSchoolsAsync(
    CancellationToken cancellationToken)
    {
        var schools = await _sender.Send(
            new ListSchoolsQuery(),
            cancellationToken);

        ViewBag.Schools = new SelectList(
            schools,
            "Id",
            "Name");
    }
    #endregion

}
