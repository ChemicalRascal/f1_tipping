namespace F1Tipping.Model.Tipping
{
    public class Player
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid AuthUserId { get; set; }
        public required PlayerStatus Status { get; set; }
        public Identity? Details { get; set; }
        public List<Guid>? AdditionalAuthedUsers { get; set; }

        public class Identity
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public required string FirstName { get; set; }
            public string? LastName { get; set; }
            public string? DisplayName { get; set; }
        }
    }

    public enum PlayerStatus
    {
        NotSet,
        Uninitialized,
        Normal,
        NotPlaying,
        Archived,
    }
}
