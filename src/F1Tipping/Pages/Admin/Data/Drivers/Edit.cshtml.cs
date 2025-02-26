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

namespace F1Tipping.Pages.Admin.Data.Drivers
{
    public class EditModel : AdminPageModel
    {
        private readonly ModelDbContext _context;

        public EditModel(ModelDbContext context)
        {
            _context = context;

            TeamsSL = new SelectList(Array.Empty<object>());
        }

        [BindProperty]
        public Driver Driver { get; set; } = default!;
        public SelectList TeamsSL { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driver = await _context.Drivers.FirstOrDefaultAsync(m => m.Id == id);
            if (driver == null)
            {
                return NotFound();
            }
            Driver = driver;

            TeamsSL = new SelectList(await _context.Teams.ToListAsync(),
                nameof(Team.Id), nameof(Team.Name), Driver.Team.Id);

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState["Driver.Team.Id"]?.ValidationState == ModelValidationState.Valid)
            {
                ModelState.Remove<EditModel>(m => m.Driver.Team.Name);
            }

            if (!ModelState.IsValid)
            {
                TeamsSL = new SelectList(await _context.Teams.ToListAsync(),
                    nameof(Team.Id), nameof(Team.Name));
                return Page();
            }

            _context.Attach(Driver).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DriverExists(Driver.Id))
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

        private bool DriverExists(Guid id)
        {
            return _context.Drivers.Any(e => e.Id == id);
        }
    }
}
