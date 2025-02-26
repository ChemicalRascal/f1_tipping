namespace F1Tipping.Model.Tipping
{
    public class Tip
    {
        public Guid Id { get; set; }
        public required Player Tipper { get; set; }
        public required Result Target { get; set; }
        public required RacingEntity Selection { get; set; }
        public required DateTimeOffset SubmittedAt { get; set; }
        public Guid? SubmittedBy_AuthUser { get; set; }

        public bool IsProxyTip => SubmittedBy_AuthUser is not null &&
            Tipper.AuthUserId == SubmittedBy_AuthUser;
    }
}
