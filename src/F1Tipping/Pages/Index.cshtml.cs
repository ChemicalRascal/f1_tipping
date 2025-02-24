using F1Tipping.Data;
using F1Tipping.Pages.Tipping;
using F1Tipping.PlayerData;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Pages
{
    [PlayerMustBeInitalized]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private ModelDbContext _modelDb;
        private UserManager<IdentityUser<Guid>> _userManager;

        [BindProperty]
        public Type? MainViewComponent { get; set; } = default;
        [BindProperty]
        public object? MainViewComponentParameters { get; set; } = default;

        public IndexModel(ILogger<IndexModel> logger, ModelDbContext modelDb, UserManager<IdentityUser<Guid>> userManager)
        {
            _logger = logger;
            _modelDb = modelDb;
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            if ((User?.Identity?.IsAuthenticated ?? false) && User.IsInRole("Player"))
            {
                return Redirect("Tipping");
            }

            return Page();
        }
    }
}
