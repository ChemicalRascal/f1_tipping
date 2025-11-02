using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Data;
using F1Tipping.Model.Tipping;
using F1Tipping.Pages.PageModels;

namespace F1Tipping.Pages.Admin.Players
{
    public class DetailsModel : AdminPageModel
    {
        private readonly ModelDbContext _context;

        public DetailsModel(IConfiguration configuration, ModelDbContext context)
            : base(configuration)
        {
            _context = context;
        }

        public Player Player { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players.FirstOrDefaultAsync(m => m.Id == id);

            if (player is not null)
            {
                Player = player;

                return Page();
            }

            return NotFound();
        }
    }
}
