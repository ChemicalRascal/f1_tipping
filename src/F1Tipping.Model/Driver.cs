namespace F1Tipping.Model;

public class Driver
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Nationality { get; set; }
    public required string Number { get; set; }
    public required EntityStatus Status { get; set; }
    public virtual ICollection<DriverTeam> DriverTeams { get; set; } = [];
    public string DisplayName { get => $"{FirstName} {LastName}"; }
    public string ShortDisplayName { get => LastName?[..3].ToUpperInvariant() ?? string.Empty; }
    public virtual bool IsSelectable => Status == EntityStatus.Active;
}