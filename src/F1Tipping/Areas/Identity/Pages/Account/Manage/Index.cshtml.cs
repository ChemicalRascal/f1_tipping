// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using F1Tipping.Common;
using F1Tipping.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser<Guid>> _userManager;
        private readonly SignInManager<IdentityUser<Guid>> _signInManager;
        private readonly ModelDbContext _modelDb;

        public IndexModel(
            UserManager<IdentityUser<Guid>> userManager,
            SignInManager<IdentityUser<Guid>> signInManager,
            ModelDbContext modelDb)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _modelDb = modelDb;
        }

        public string Username { get; set; } = string.Empty;

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public InputModel? Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string? PhoneNumber { get; set; }
            [Display(Name = "First name")]
            public required string FirstName { get; set; }
            [Display(Name = "Last name")]
            public string? LastName { get; set; }
            [Display(Name = "Display name")]
            public string? DisplayName { get; set; }
        }

        private async Task LoadAsync(IdentityUser<Guid> user)
        {
            var userName = (await _userManager.GetUserNameAsync(user)) ?? string.Empty;
            var phoneNumber = (await _userManager.GetPhoneNumberAsync(user));
            var player = await _modelDb.Players.Where(p => p.AuthUserId == user.Id).FirstOrDefaultAsync();
            if (player?.Details is null)
            {
                StatusMessage = "Unexpected error when trying to set player details.";
            }

            Username = userName;
            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FirstName = player?.Details?.FirstName ?? string.Empty,
                LastName = player?.Details?.LastName,
                DisplayName = player?.Details?.DisplayName,
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input!.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            var player = await _modelDb.Players.Where(p => p.AuthUserId == user.Id).FirstOrDefaultAsync();
            if (player?.Details is null)
            {
                StatusMessage = "Unexpected error when trying to set player details.";
            }
            else
            {
                player.Details.FirstName = Input.FirstName.Trim();
                player.Details.LastName = Input.LastName?.Trim().NullIfEmpty();
                player.Details.DisplayName = Input.DisplayName?.Trim().NullIfEmpty();
                _modelDb.Update(player);
                await _modelDb.SaveChangesAsync();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
