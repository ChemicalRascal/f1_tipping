using F1Tipping.Data;
using F1Tipping.Pages.Tipping;
using F1Tipping.PlayerData;
using F1Tipping.Tipping;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace F1Tipping.Pages
{
    [PlayerMustBeInitalized]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private ModelDbContext _modelDb;
        private UserManager<IdentityUser<Guid>> _userManager;
        private IConfiguration _configuration;

        [BindProperty]
        public Type? MainViewComponent { get; set; } = default;
        [BindProperty]
        public object? MainViewComponentParameters { get; set; } = default;
        [BindProperty]
        public string WelcomeTitle { get; set; } = default!;
        [BindProperty]
        public string WelcomeSubtitle { get; set; } = default!;
        [BindProperty]
        public DateTimeOffset NextEventDeadline { get; set; } = default!;
        [BindProperty]
        public string VapidPublicKey { get; set; } = default!;

        public IndexModel(ILogger<IndexModel> logger, ModelDbContext modelDb, UserManager<IdentityUser<Guid>> userManager, IConfiguration configuration)
        {
            _logger = logger;
            _modelDb = modelDb;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGet()
        {
            WelcomeTitle = "Welcome to the F1 Tipping Competition!";
            NextEventDeadline = await DeadlineService.GetNextDeadlineAsync(_modelDb);

            IdentityUser<Guid>? dbUser = null;
            if (User is not null)
            {
                dbUser = await _userManager.GetUserAsync(User);
            }

            if (dbUser is not null)
            {
                WelcomeSubtitle = "Click \"Tipping\" above to tip some races!";
            }
            else
            {
                WelcomeSubtitle = "Make an account, log in, tip some races!";
            }

            VapidPublicKey = _configuration["VAPID:publicKey"];

            return Page();
        }
    }
}
