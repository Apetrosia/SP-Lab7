using GreenSwampApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GreenSwampApp.Pages;

public class SubscribeModel : PageModel
{
    private readonly IEmailService _emailService;

    public SubscribeModel(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [BindProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            TempData["SubscribeError"] = "Please enter a valid email address.";
            return RedirectToPage("/Index");
        }

        await _emailService.SendSubscriptionConfirmationAsync(Email);
        TempData["SubscribeSuccess"] = "Check your inbox to confirm subscription!";
        return RedirectToPage("/Index");
    }
}