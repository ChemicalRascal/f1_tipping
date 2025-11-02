using F1Tipping.Data;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Pages.Admin.Users
{
    public class LockdownUserModel : BasePageModel
    {
        private readonly AppDbContext _context;

        [BindProperty]
        public UserView UserToLock { get; set; } = default!;

        public LockdownUserModel(
            IConfiguration configuration,
            AppDbContext context
            ) : base(configuration)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGet(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            UserToLock = new UserView(user.Id, user.Email);

            throw new NotImplementedException();
        }
    }
}
