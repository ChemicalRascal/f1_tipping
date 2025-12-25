using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Data;
using F1Tipping.Model;
using F1Tipping.Pages.PageModels;
using Microsoft.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace F1Tipping.Pages.Admin.Data.DriverTeams
{
    public class EditModel : AdminPageModel
    {
        private readonly ModelDbContext _context;

        public EditModel(IConfiguration configuration, ModelDbContext context)
            : base(configuration)
        {
            _context = context;

            TeamsSL = new SelectList(Array.Empty<object>());
            DriversSL = new SelectList(Array.Empty<object>());
            StatusSL = new SelectList(Array.Empty<object>());
        }

        [BindProperty]
        public DriverTeam DriverTeam { get; set; } = default!;
        public SelectList TeamsSL { get; set; } = default!;
        public SelectList DriversSL { get; set; } = default!;
        public SelectList StatusSL { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driverTeam = await _context.DriverTeams.FirstOrDefaultAsync(m => m.Id == id);
            if (driverTeam == null)
            {
                return NotFound();
            }
            DriverTeam = driverTeam;

            await BuildSelectLists(DriverTeam.Status);
            return Page();
        }

        private async Task BuildSelectLists(EntityStatus selectedStatus)
        {
            TeamsSL = new SelectList(await _context.Teams.ToListAsync(),
                nameof(Team.Id), nameof(Team.Name), DriverTeam.Team.Id);
            DriversSL = new SelectList(await _context.Drivers.ToListAsync(),
                nameof(Driver.Id), nameof(Driver.DisplayName), DriverTeam.Driver.Id);

            StatusSL = new SelectList(Enum.GetValues<EntityStatus>()
                .Cast<EntityStatus>()
                .Select(v => new SelectListItem
                {
                    Text = v.ToString(),
                    Value = ((int)v).ToString(),
                }), "Value", "Text", ((int)selectedStatus).ToString());
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            // There's gotta be a better way to do this.
            if (ModelState["DriverTeam.Team.Id"]?.ValidationState == ModelValidationState.Valid)
            {
                ModelState.Remove<EditModel>(m => m.DriverTeam.Team.Name);
            }
            if (ModelState["DriverTeam.Driver.Id"]?.ValidationState == ModelValidationState.Valid)
            {
                ModelState.Remove<EditModel>(m => m.DriverTeam.Driver.FirstName);
                ModelState.Remove<EditModel>(m => m.DriverTeam.Driver.LastName);
                ModelState.Remove<EditModel>(m => m.DriverTeam.Driver.Nationality);
                ModelState.Remove<EditModel>(m => m.DriverTeam.Driver.Number);
            }

            if (!ModelState.IsValid)
            {
                await BuildSelectLists(EntityStatus.NotSet);
                return Page();
            }

            _context.Attach(DriverTeam).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DriverTeamExists(DriverTeam.Id))
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

        private bool DriverTeamExists(Guid id)
        {
            return _context.DriverTeams.Any(e => e.Id == id);
        }
    }
}
