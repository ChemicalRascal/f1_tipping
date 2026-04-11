using F1Tipping.Common;
using F1Tipping.Data;
using F1Tipping.Data.AppModel;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Pages.Admin.Users;

public class PushSubsModel : AdminPageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly AppDbContext _appDb;
    private readonly ModelDbContext _modelDb;
    private readonly UserManager<User> _userManager;

    public PushSubsModel(
        IConfiguration configuration,
        ILogger<IndexModel> logger,
        AppDbContext appDb,
        ModelDbContext modelDb,
        UserManager<User> userManager
        ) : base(configuration)
    {
        _logger = logger;
        _appDb = appDb;
        _modelDb = modelDb;
        _userManager = userManager;
    }

    public class PushSubView
    {
        public required string UserName;
        public required string ScheduleType;
        public TimeSpan? StartOffset;
        public TimeSpan? MinGap;
        public bool? StaleNotify;
        public required string DeviceEndpoint;
        public DateTime SubCreated;
        public DateTime? SubLastCheck;
    }

    [BindProperty]
    public IList<PushSubView> PushSubs { get; set; } = default!;
    [BindProperty]
    public string? StatusMessage { get; set; } = null;

    private async Task BuildViewListsAsync()
    {
        var pushSubs = await _appDb.UserPushNotificationSubscriptions
            .Include(s => s.User)
            .ThenInclude(u => u.Settings)
            .ThenInclude(s => s.NotificationsSettings)
            .OrderBy(s => s.User.NormalizedUserName).ToListAsync();

        PushSubs = pushSubs.Select(s =>
        {
            return new PushSubView()
            {
                UserName = s.User?.UserName ?? "null",
                ScheduleType = s.User?.Settings.NotificationsSettings?.ScheduleType.DisplayName() ?? "null",
                StartOffset = s.User?.Settings.NotificationsSettings?.TipDeadlineStartOffset,
                MinGap = s.User?.Settings.NotificationsSettings?.MinimumTimeBetweenNotifications,
                StaleNotify = s.User?.Settings.NotificationsSettings?.NotifyForOldTips,
                DeviceEndpoint = s.DeviceEndpoint[0..60],
                SubCreated = s.Created,
                SubLastCheck = s.LastCheck,
            };
        }).ToList();
    }

    public async Task OnGetAsync()
    {
        await BuildViewListsAsync();
    }
}
