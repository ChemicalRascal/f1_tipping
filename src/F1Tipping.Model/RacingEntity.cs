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
        public abstract bool IsSelectable { get; }
        public virtual int? GetListOrder() => ListOrder;
    }

    public class DriverTeam : RacingEntity
    {
        [Display(Name = "Driver")]
        public required Driver Driver { get; set; }
        [Display(Name = "Team")]
        public required Team Team { get; set; }
        [Display(Name = "Status")]
        public AssociationStatus Status { get; set; } = AssociationStatus.NotSet;
        public override string DisplayName { get => $"{Driver.DisplayName} - {Team?.DisplayName}"; }
        public override string ShortDisplayName { get => Driver.ShortDisplayName; }
        public override int? GetListOrder() => Team.GetListOrder() * 100 + base.GetListOrder();
        public override bool IsSelectable => Status == AssociationStatus.Active;
    }

    public class Team : RacingEntity
    {
        public required string Name { get; set; }
        public override string DisplayName { get => $"{Name}"; }
        public override string ShortDisplayName { get => $"{Name}"; }
        public override bool IsSelectable => true;
    }

    public enum AssociationStatus
    {
        NotSet,
        Active,
        Inactive,
    }
}