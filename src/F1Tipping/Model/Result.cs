namespace F1Tipping.Model
{
    public class Result
    {
        public Guid Id { get; set; }
        public Event? Event { get; set; }
        public ResultType Type { get; set; }
        public RacingEntity? ResultHolder { get; set; }
    }
}
