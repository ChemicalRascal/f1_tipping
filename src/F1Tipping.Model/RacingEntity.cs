using System.ComponentModel.DataAnnotations;

namespace F1Tipping.Model;

public abstract class RacingEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Display(Name = "List Index")]
    public int? ListOrder { get; set; }
    [Display(Name = "Status")]
    public required EntityStatus Status { get; set; }
    [Display(Name = "Name")]
    public abstract string DisplayName { get; }
    public abstract string ShortDisplayName { get; }
    public virtual bool IsSelectable => Status == EntityStatus.Active;
    public virtual int? GetListOrder() => ListOrder;
}

public class DriverTeam : RacingEntity
{
    [Display(Name = "Driver")]
    public required Driver Driver { get; set; }
    [Display(Name = "Team")]
    public required Team Team { get; set; }
    public override string DisplayName { get => $"{Driver.DisplayName} - {Team.DisplayName}"; }
    public override string ShortDisplayName { get => Driver.ShortDisplayName; }
    public override int? GetListOrder() => Team.GetListOrder() * 100 + base.GetListOrder();
}

public class Team : RacingEntity
{
    public required string Name { get; set; }
    public virtual ICollection<DriverTeam>? DriverTeams { get; set; }
    public override string DisplayName { get => $"{Name}"; }
    public override string ShortDisplayName { get => $"{Name}"; }
}

public enum EntityStatus
{
    NotSet,
    Active,
    Inactive,
}