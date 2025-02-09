namespace F1Tipping.Model
{
    public class Round
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public DateTime Date { get; set; }
    }
}
