using F1Tipping.Data.AppModel;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;

namespace F1Tipping.Areas.Identity.Pages.Account.Manage;

public class PushNotificationsModel : BasePageModel
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<PushNotificationsModel> _logger;

    private readonly List<TimeOption> StartOptions = [
        new(TimeSpan.FromMinutes(30), "30 min"),
        new(TimeSpan.FromHours(1), "1 hr"),
        new(TimeSpan.FromHours(2), "2 hrs"),
        new(TimeSpan.FromHours(4), "4 hrs"),
        new(TimeSpan.FromHours(8), "8 hrs"),
        new(TimeSpan.FromHours(12), "12 hrs"),
        new(TimeSpan.FromDays(1), "1 day"),
        new(TimeSpan.FromDays(2), "2 days", IsDefault: true),
        new(TimeSpan.FromDays(4), "4 days"),
        ];

    private readonly List<TimeOption> MinGapOptions = [
        new(TimeSpan.FromMinutes(5), "5 min"),
        new(TimeSpan.FromMinutes(10), "10 min"),
        new(TimeSpan.FromMinutes(15), "15 min"),
        new(TimeSpan.FromMinutes(30), "30 min"),
        new(TimeSpan.FromHours(1), "1 hr", IsDefault: true),
        new(TimeSpan.FromHours(2), "2 hrs"),
        new(TimeSpan.FromHours(4), "4 hrs"),
        new(TimeSpan.FromHours(8), "8 hrs"),
        new(TimeSpan.FromHours(12), "12 hrs"),
        ];

    public PushNotificationsModel(
        IConfiguration configuration,
        UserManager<User> userManager,
        ILogger<PushNotificationsModel> logger
        ) : base(configuration)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public override void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
        base.OnPageHandlerSelected(context);
        ViewData["StartOptions"] = StartOptions;
        ViewData["MinGapOptions"] = MinGapOptions;
    }

    [BindProperty]
    public required InputModel Input { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public class InputModel
    {
        [HiddenInput]
        public bool NotificationsEnabled { get; set; }
        [Display(Name = "Start X Before Quali:")]
        public TimeSpan? TipDeadlineStartOffset { get; set; }
        [Display(Name = "At Least X Between Reminders:")]
        public TimeSpan? MinimumTimeBetweenNotifications { get; set; }
        [Display(Name = "Remind Me If Tips Are \"Stale\"")]
        public bool NotifyForOldTips { get; set; }
        [Display(Name = "Reminder Schedule Structure")]
        public NotificationsScheduleType? ScheduleType { get; set; }
    }

    public record TimeOption(TimeSpan Time, string Label, bool IsDefault = false);

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        Input = new()
        {
            NotificationsEnabled = user.Settings.NotificationsSettings is not null,
            TipDeadlineStartOffset = user.Settings.NotificationsSettings?.TipDeadlineStartOffset,
            MinimumTimeBetweenNotifications = user.Settings.NotificationsSettings?.MinimumTimeBetweenNotifications,
            NotifyForOldTips = user.Settings.NotificationsSettings?.NotifyForOldTips ?? false,
            ScheduleType = user.Settings.NotificationsSettings?.ScheduleType,
        };

        return Page();
    }

    private void ValidateInputModel()
    {
        if (Input.NotificationsEnabled)
        {
            if (Input.ScheduleType == NotificationsScheduleType.NotSet)
            {
                ModelState.AddModelError("Input.ScheduleType", "Must be set.");
            }
            if (Input.TipDeadlineStartOffset is null)
            {
                ModelState.AddModelError("Input.TipDeadlineStartOffset", "Must be a valid TimeSpan.");
            }
            if (Input.MinimumTimeBetweenNotifications is null)
            {
                ModelState.AddModelError("Input.MinimumTimeBetweenNotifications", "Must be a valid TimeSpan.");
            }

            if (Input.MinimumTimeBetweenNotifications <
                MinGapOptions.Select(t => t.Time).Min())
            {
                ModelState.AddModelError("Input.MinimumTimeBetweenNotifications", "Resulting gap is too short.");
            }
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        ValidateInputModel();
        if (!ModelState.IsValid)
        {
            Input.NotificationsEnabled = user.Settings.NotificationsSettings is not null;
            return Page();
        }

        if (!Input.NotificationsEnabled)
        {
            user.Settings.NotificationsSettings = null;
        }
        else
        {
            if (user.Settings.NotificationsSettings is null)
            {
                user.Settings.NotificationsSettings = new();
            }
            user.Settings.NotificationsSettings.ScheduleType = Input.ScheduleType!.Value;
            user.Settings.NotificationsSettings.NotifyForOldTips = Input.NotifyForOldTips;
            user.Settings.NotificationsSettings.MinimumTimeBetweenNotifications = Input.MinimumTimeBetweenNotifications!.Value;
            user.Settings.NotificationsSettings.TipDeadlineStartOffset = Input.TipDeadlineStartOffset!.Value;
        }

        await _userManager.UpdateAsync(user);
        StatusMessage = "Settings updated.";
        return RedirectToPage();
    }
}
