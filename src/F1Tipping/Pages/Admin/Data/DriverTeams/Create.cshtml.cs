using F1Tipping.Data;
using F1Tipping.Model;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Pages.Admin.Data.DriverTeams
{
    public class CreateModel : AdminPageModel
    {
        private readonly ModelDbContext _context;

        public CreateModel(IConfiguration configuration, ModelDbContext context)
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

        public async Task<IActionResult> OnGetAsync()
        {
            await BuildSelectLists();
            return Page();
        }

        private async Task BuildSelectLists()
        {
            TeamsSL = new SelectList(await _context.Teams.ToListAsync(),
                nameof(Team.Id), nameof(Team.Name));
            DriversSL = new SelectList(await _context.Drivers.ToListAsync(),
                nameof(Driver.Id), nameof(Driver.DisplayName));

            StatusSL = new SelectList(Enum.GetValues<AssociationStatus>()
                .Cast<AssociationStatus>()
                .Select(v => new SelectListItem
                {
                    Text = v.ToString(),
                    Value = ((int)v).ToString(),
                }), "Value", "Text", ((int)AssociationStatus.NotSet).ToString());
        }

        public async Task<IActionResult> OnPostAsync()
        {
            DriverTeam.Id = Guid.NewGuid();
            ModelState.Remove<CreateModel>(m => m.DriverTeam.Id);
            if (ModelState["DriverTeam.Team.Id"]?.ValidationState == ModelValidationState.Valid)
            {
                ModelState.Remove<CreateModel>(m => m.DriverTeam.Team.Name);
            }
            if (ModelState["DriverTeam.Driver.Id"]?.ValidationState == ModelValidationState.Valid)
            {
                ModelState.Remove<CreateModel>(m => m.DriverTeam.Driver.FirstName);
                ModelState.Remove<CreateModel>(m => m.DriverTeam.Driver.LastName);
                ModelState.Remove<CreateModel>(m => m.DriverTeam.Driver.Nationality);
                ModelState.Remove<CreateModel>(m => m.DriverTeam.Driver.Number);
            }

            if (!ModelState.IsValid)
            {
                await BuildSelectLists();
                return Page();
            }

            _context.Attach(DriverTeam).State = EntityState.Added;

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
