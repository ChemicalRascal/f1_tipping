using Microsoft.AspNetCore.Mvc;
using F1Tipping.Pages.PageModels;
using F1Tipping.Data;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Pages.Admin.System
{
    public class IndexModel : AdminPageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly AppDbContext _appDb;

        public IndexModel(
            ILogger<IndexModel> logger,
            AppDbContext appDb
            )
        {
            _logger = logger;
            _appDb = appDb;
        }

        [BindProperty]
        public SystemSettings SystemSettings { get; set; } = default!;

        public async Task<IActionResult> OnGet()
        {
            SystemSettings = await _appDb.GetSystemSettingsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var existingSettings = await _appDb.SystemSettings.SingleOrDefaultAsync();

            if (existingSettings is null)
            {
                await _appDb.AddAsync(SystemSettings);
                await _appDb.SaveChangesAsync();
            }
            else
            {
                existingSettings.RegistrationEnabled = SystemSettings.RegistrationEnabled;
                _appDb.Update(existingSettings);
                await _appDb.SaveChangesAsync();
            }

            return Redirect("/Index");
        }
    }
}
