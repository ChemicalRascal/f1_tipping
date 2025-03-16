using System.ComponentModel.DataAnnotations;

namespace F1Tipping.Model
{
    public abstract class RacingEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Display(Name = "List Index")]
        public int? ListOrder { get; set; }
        [Display(Name = "Name")]
        public abstract string DisplayName { get; }
        public abstract string ShortDisplayName { get; }
        public virtual int? GetListOrder() => ListOrder;
    }

    public class Driver : RacingEntity
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Nationality { get; set; }
        public required string Number { get; set; }
        public required Team Team { get; set; }
        public override string DisplayName { get => $"{FirstName} {LastName} - {Team?.DisplayName}"; }
        public override string ShortDisplayName { get => LastName[..3].ToUpperInvariant(); }
        public override int? GetListOrder() => Team.GetListOrder() * 100 + base.GetListOrder();
    }

    public class Team : RacingEntity
    {
        public required string Name { get; set; }
        public override string DisplayName { get => $"{Name}"; }
        public override string ShortDisplayName { get => $"{Name}"; }
    }
}