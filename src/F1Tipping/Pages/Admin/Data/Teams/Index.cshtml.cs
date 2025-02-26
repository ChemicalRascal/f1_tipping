using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Data;
using F1Tipping.Model;
using Microsoft.AspNetCore.Authorization;
using F1Tipping.Pages.PageModels;

namespace F1Tipping.Pages.Admin.Data.Teams
{
    public class IndexModel : AdminPageModel
    {
        private readonly ModelDbContext _context;

        public IndexModel(ModelDbContext context)
        {
            _context = context;
        }

        public IList<Team> Team { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Team = await _context.Teams.ToListAsync();
        }
    }
}
