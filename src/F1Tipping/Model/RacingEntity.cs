namespace F1Tipping.Model
{
    public abstract class RacingEntity
    {
        public Guid Id { get; set; }
    }

    public class Driver : RacingEntity
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required Team Team { get; set; }
    }

    public class Team : RacingEntity
    {
        public required string Name { get; set; }
    }
}