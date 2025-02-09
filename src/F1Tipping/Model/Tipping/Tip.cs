namespace F1Tipping.Model.Tipping
{
    public class Tip
    {
        public Guid Id { get; set; }
        public required Player Tipper { get; set; }
        public required Result Target { get; set; }
    }
}
