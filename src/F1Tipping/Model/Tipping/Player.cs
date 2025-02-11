namespace F1Tipping.Model.Tipping
{
    public class Player
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid AuthUserId { get; set; }
    }
}
