using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Data;
using F1Tipping.Model.Tipping;
using F1Tipping.Pages.PageModels;

namespace F1Tipping.Pages.Admin.Players
{
    public class EditModel : AdminPageModel
    {
        private readonly ModelDbContext _context;

        public EditModel(IConfiguration configuration, ModelDbContext context)
            : base(configuration)
        {
            _context = context;
        }

        [BindProperty]
        public Player Player { get; set; } = default!;
        [BindProperty]
        public IEnumerable<SelectListItem> Statuses { get; set; } =
            Enum.GetValues<PlayerStatus>()
            .Select(s => new SelectListItem(s.ToString(), ((int)s).ToString()));

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players.FirstOrDefaultAsync(m => m.Id == id);
            if (player == null)
            {
                return NotFound();
            }
            Player = player;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Player).State = EntityState.Modified;
            if (Player.Details is not null)
            {
                _context.Attach(Player.Details).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerExists(Player.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool PlayerExists(Guid id)
        {
            return _context.Players.Any(e => e.Id == id);
        }
    }
}
