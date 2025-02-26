namespace F1Tipping.Model
{
    public abstract class RacingEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public abstract string DisplayName { get; }
    }

    public class Driver : RacingEntity
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required Team Team { get; set; }
        public override string DisplayName { get => $"{FirstName} {LastName} - {Team.DisplayName}"; }
    }

    public class Team : RacingEntity
    {
        public required string Name { get; set; }
        public override string DisplayName { get => $"{Name}"; }
    }
}