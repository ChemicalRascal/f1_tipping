namespace F1Tipping.Data.AppModel;

public class UserSettings
{
    public NotificationsSettings? NotificationsSettings { get; set; }
}

public enum NotificationsScheduleType
{
    NotSet,
    ExponentialDecay,
    FixedPeriods,
}

public class NotificationsSettings
{
    public TimeSpan TipDeadlineStartOffset { get; set; }
    public bool NotifyForOldTips { get; set; }
    public NotificationsScheduleType ScheduleType { get; set; }
}