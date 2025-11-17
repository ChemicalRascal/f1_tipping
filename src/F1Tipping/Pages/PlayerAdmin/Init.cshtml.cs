using Microsoft.AspNetCore.Mvc;
using F1Tipping.Data;
using F1Tipping.Data.AppModel;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace F1Tipping.Pages.PlayerAdmin
{
    public class InitModel : PlayerPageModel
    {
        public InitModel(
            IConfiguration configuration,
            UserManager<User> userManager,
            AppDbContext appDb,
            ModelDbContext modelDb
            ) : base(configuration, userManager, appDb, modelDb)
        { }

        public class DetailsEditModel
        {
            [Display(Name ="First Name", Description = "Non-optional.")]
            public string? FirstName { get; set; }
            [Display(Name ="Last Name", Description = "Optional.")]
            public string? LastName { get; set; }
            [Display(Name ="Display Name", Description = "Optional. Overrides your First Name on other people's UI.")]
            public string? DisplayName { get; set; }
        }

        [BindProperty]
        public required DetailsEditModel Details { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await SetUserAsync(User);

            if (Player?.Details is not null)
            {
                Details.FirstName = Player.Details.FirstName;
                Details.LastName = Player.Details.LastName;
                Details.DisplayName = Player.Details.DisplayName;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await SetUserAsync(User);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (Player is not null && Player.AuthUserId != AuthUser!.Id)
            {
                return BadRequest();
            }

            if (await _modelDb.CreatePlayerIfNeededAsync(AuthUser!))
            {
                await SetUserAsync(User);
            }

            Player!.Details = new()
            {
                // TODO: Verification
                // TODO: Some sort of .NullIfEmpty() extension?
                FirstName = Details.FirstName ?? string.Empty,
                LastName = !Details.LastName.IsNullOrEmpty()
                            ? Details.LastName : null,
                DisplayName = !Details.DisplayName.IsNullOrEmpty()
                            ? Details.DisplayName : null,
            };
            Player.Status = Model.Tipping.PlayerStatus.Normal;
            _modelDb.Update(Player);
            await _modelDb.SaveChangesAsync();

            return Redirect("/");
        }
    }
}
