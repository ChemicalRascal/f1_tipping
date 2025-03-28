namespace F1Tipping.Model
{
    public class Driver
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Nationality { get; set; }
        public required string Number { get; set; }
        public string DisplayName { get => $"{FirstName} {LastName}"; }
        public string ShortDisplayName { get => LastName?[..3].ToUpperInvariant() ?? string.Empty; }
    }
}
