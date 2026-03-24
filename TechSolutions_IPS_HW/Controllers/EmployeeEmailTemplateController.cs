using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechSolutions_IPS_HW.Models.ViewModels;
using TechSolutions_IPS_HW.Services.Interfaces;

namespace TechSolutions_IPS_HW.Controllers;


//This page is to allow employees to create their own custom email templates
//it also ensures that only the employee who created a specific template can access their own email templates
[Authorize(Roles = "Employee")]
[Route("employee/email-templates")]
public class EmployeeEmailTemplateController : Controller
{
    private readonly IEmployeeEmailTemplateService _templateService;

    public EmployeeEmailTemplateController(IEmployeeEmailTemplateService templateService)
    {
        _templateService = templateService;
    }

    //Get user context and load authenticated user's custome email templates
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Forbid();

        var items = await _templateService.GetMyTemplatesAsync(userId);
        return View(items);
    }

    //Post new email template and persist it in database binding it to the specified user
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeEmailTemplateFormModel model)
    {
        if (!ModelState.IsValid)
            return await ReturnIndexWithValidationAsync(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Forbid();

        var result = await _templateService.CreateAsync(userId, model);
        if (!result.Success)
            return await ReturnIndexWithErrorAsync(result.Error, model);

        TempData["StatusMessage"] = "Template created.";
        return RedirectToAction(nameof(Index));
    }

    //Loads a specific email template so user can edit it.
    [HttpGet("edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Forbid();

        var template = await _templateService.GetMyTemplateAsync(id, userId);
        if (template == null) return NotFound();

        var model = new EmployeeEmailTemplateFormModel
        {
            Id = template.Id,
            Name = template.Name,
            Subject = template.Subject,
            BodyHtml = template.BodyHtml
        };

        return View(model);
    }

    //Post to persist email template changes
    [HttpPost("edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EmployeeEmailTemplateFormModel model)
    {
        model.Id = id;

        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Forbid();

        var result = await _templateService.UpdateAsync(userId, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(model);
        }

        TempData["StatusMessage"] = "Template updated.";
        return RedirectToAction(nameof(Index));
    }


    //Deletes a specified suer;s email template, we opted not to soft delete this entity but remove it completely
    //as it only relates to a specified user and no other users.
    [HttpPost("delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Forbid();

        var result = await _templateService.DeleteAsync(userId, id);
        TempData["StatusMessage"] = result.Success ? "Template deleted." : result.Error;

        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ReturnIndexWithValidationAsync(EmployeeEmailTemplateFormModel model)
    {
        ViewData["CreateModel"] = model;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Forbid();

        var items = await _templateService.GetMyTemplatesAsync(userId);
        return View("Index", items);
    }

    private async Task<IActionResult> ReturnIndexWithErrorAsync(string error, EmployeeEmailTemplateFormModel model)
    {
        ModelState.AddModelError(string.Empty, error);
        return await ReturnIndexWithValidationAsync(model);
    }
}
