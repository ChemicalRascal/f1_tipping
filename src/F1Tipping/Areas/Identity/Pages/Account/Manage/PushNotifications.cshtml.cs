// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace F1Tipping.Areas.Identity.Pages.Account.Manage;

public class PushNotificationsModel : BasePageModel
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly ILogger<PushNotificationsModel> _logger;

    public PushNotificationsModel(
        IConfiguration configuration,
        UserManager<IdentityUser<Guid>> userManager,
        ILogger<PushNotificationsModel> logger
        ) : base(configuration)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    public class InputModel
    {
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        return RedirectToPage();
    }
}
