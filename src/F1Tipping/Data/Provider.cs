namespace F1Tipping.Data
{
    public record Provider(string Name, string Assembly)
    {
        public static Provider SqlServer = new(nameof(SqlServer), typeof(SqlServer.Marker).Assembly.GetName().Name!);
        public static Provider Postgres = new(nameof(Postgres), typeof(Postgres.Marker).Assembly.GetName().Name!);
    }
}
