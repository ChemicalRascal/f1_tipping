using System.ComponentModel.DataAnnotations;

namespace F1Tipping.Data.AppModel;

public class UserSettings
{
    public NotificationsSettings? NotificationsSettings { get; set; }
}

public enum NotificationsScheduleType
{
    [Display(Name = "Not Set!")]
    NotSet,
    [Display(Name = "Exponential Gap Decay", Description = "After sending a notification, schedules another notification halfway between the current time and the deadline time. Intended to get progrssively more annoying.")]
    ExponentialDecay,
    [Display(Name = "Fixed Gap", Description = "After sending a notification, sechedules another notification after the set time between notifications.")]
    FixedPeriods,
}

public class NotificationsSettings
{
    public TimeSpan TipDeadlineStartOffset { get; set; }
    public TimeSpan MinimumTimeBetweenNotifications { get; set; }
    public bool NotifyForOldTips { get; set; }
    public NotificationsScheduleType ScheduleType { get; set; }
}