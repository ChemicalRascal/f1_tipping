using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Data;
using F1Tipping.Model.Tipping;
using Microsoft.AspNetCore.Authorization;
using F1Tipping.Pages.PageModels;

namespace F1Tipping.Pages.Admin.Players
{
    public class IndexModel : AdminPageModel
    {
        private readonly ModelDbContext _context;

        public IndexModel(IConfiguration configuration, ModelDbContext context)
            : base(configuration)
        {
            _context = context;
        }

        public IList<Player> Player { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Player = await _context.Players.ToListAsync();
        }
    }
}
