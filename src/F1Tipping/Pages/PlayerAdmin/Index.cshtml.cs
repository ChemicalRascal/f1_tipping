using Microsoft.AspNetCore.Mvc;
using F1Tipping.Model.Tipping;
using F1Tipping.Data;
using F1Tipping.Data.AppModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using F1Tipping.PlayerData;
using F1Tipping.Pages.PageModels;

namespace F1Tipping.Pages.PlayerAdmin;

[PlayerMustBeInitalized]
public class IndexModel : BasePageModel
{
    private readonly ModelDbContext _modelDb;
    private readonly UserManager<User> _userManager;

    public IndexModel(
        IConfiguration configuration,
        ModelDbContext modelDb,
        UserManager<User> userManager
        ) : base(configuration)
    {
        _modelDb = modelDb;
        _userManager = userManager;
    }

    [BindProperty]
    public Player? Player { get; set; }

    public async Task OnGet()
    {
        var user = await _userManager.GetUserAsync(User);
        Player = await _modelDb.Players.SingleOrDefaultAsync(p => p.AuthUserId == user!.Id);
    }
}
