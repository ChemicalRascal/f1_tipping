namespace F1Tipping.Model
{
    public class Result
    {
        public required Event Event { get; set; }
        public ResultType Type { get; set; }
        public RacingEntity? ResultHolder { get; set; }
        public DateTime? Set { get; set; }
        public Guid? SetByAuthUser { get; set; }
    }
}
