using Microsoft.AspNetCore.Mvc;
using F1Tipping.Data;
using F1Tipping.Model.Tipping;
using F1Tipping.Pages.PageModels;

namespace F1Tipping.Pages.Admin.Players
{
    public class CreateModel : AdminPageModel
    {
        private readonly ModelDbContext _context;

        public CreateModel(IConfiguration configuration, ModelDbContext context)
            : base(configuration)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Player Player { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Players.Add(Player);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
