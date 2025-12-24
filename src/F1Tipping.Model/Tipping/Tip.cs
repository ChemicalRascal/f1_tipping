namespace F1Tipping.Model.Tipping;

public class Tip : IThinTip
{
    public Guid Id { get; set; }
    public required Player Tipper { get; set; }
    public required Result Target { get; set; }
    public required RacingEntity Selection { get; set; }
    public required DateTimeOffset SubmittedAt { get; set; }
    public Guid? SubmittedBy_AuthUser { get; set; }

    public bool IsProxyTip => SubmittedBy_AuthUser is not null &&
        Tipper.AuthUserId == SubmittedBy_AuthUser;

    ResultType IThinTip.Type => Target.Type;
    Guid IThinTip.Selection => Selection.Id;
}

public interface IThinTip
{
    public ResultType Type { get; }
    public Guid Selection { get; }
}
