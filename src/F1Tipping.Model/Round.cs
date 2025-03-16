namespace F1Tipping.Model
{
    public class Round
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Season Season { get; set; }
        public required int Index { get; set; }
        public required string Title { get; set; }
        public required DateTimeOffset StartDate { get; set; }
        public required IList<Race> Events { get; set; }
    }
}
