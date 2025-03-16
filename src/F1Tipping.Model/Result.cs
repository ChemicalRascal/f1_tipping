namespace F1Tipping.Model
{
    public class Result
    {
        public required Event Event { get; set; }
        public required ResultType Type { get; set; }
        public RacingEntity? ResultHolder { get; set; }
        public DateTimeOffset? Set { get; set; }
        public Guid? SetByAuthUser { get; set; }
        public virtual bool EntityInResult(RacingEntity entity) =>
            entity == ResultHolder;
    }

    public class MultiEntityResult : Result
    {
        public List<RacingEntity>? ResultHolders { get; set; } = new();
        public override bool EntityInResult(RacingEntity entity) =>
            ResultHolders?.Contains(entity) ?? false;
    }
}
