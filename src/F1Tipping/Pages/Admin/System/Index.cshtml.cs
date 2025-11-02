using Microsoft.AspNetCore.Mvc;
using F1Tipping.Pages.PageModels;
using F1Tipping.Data;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Platform;

namespace F1Tipping.Pages.Admin.System
{
    public class IndexModel : AdminPageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly AppDbContext _appDb;

        public IndexModel(
            IConfiguration configuration,
            ILogger<IndexModel> logger,
            AppDbContext appDb
            ) : base(configuration)
        {
            _logger = logger;
            _appDb = appDb;
        }

        [BindProperty]
        public SystemSettings SystemSettings { get; set; } = default!;

        public async Task<IActionResult> OnGet()
        {
            SystemSettings = await SystemDataService.GetSystemSettingsAsync(_appDb);
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Skip cache here.
            var existingSettings = await _appDb.SystemSettings.SingleOrDefaultAsync();

            if (existingSettings is null)
            {
                await _appDb.AddAsync(SystemSettings);
                await _appDb.SaveChangesAsync();
                SystemDataService.ExpireSettings();
            }
            else
            {
                existingSettings.RegistrationEnabled = SystemSettings.RegistrationEnabled;
                _appDb.Update(existingSettings);
                await _appDb.SaveChangesAsync();
                SystemDataService.ExpireSettings();
            }

            return Redirect("/Index");
        }
    }
}
