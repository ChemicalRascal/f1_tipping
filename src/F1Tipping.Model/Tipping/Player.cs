namespace F1Tipping.Model.Tipping;

public class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid AuthUserId { get; set; }
    public required PlayerStatus Status { get; set; }
    public Identity? Details { get; set; }
    public List<Guid>? AdditionalAuthedUsers { get; set; }

    public class Identity
    {
        public required string FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DisplayName { get; set; }
        public string DisplayOrFirstName { get => DisplayName ?? FirstName; }

        public string GetModifiedDisplayName(Func<string, string>? displayNameModifier = null, Func<string, string>? firstNameModifier = null)
        {
            if (DisplayName is not null)
            {
                return displayNameModifier is not null ? displayNameModifier(DisplayName) : DisplayName;
            }
            return firstNameModifier is not null ? firstNameModifier(FirstName) : FirstName;
        }
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
