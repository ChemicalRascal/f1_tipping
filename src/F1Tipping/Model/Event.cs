using System.Diagnostics;

namespace F1Tipping.Model
{
    public abstract class Event
    {
        public Guid Id { get; set; }
    }

    public class Race : Event
    {
        public required Round Weekend { get; set; }
        public RaceType Type { get; set; }
    }

    public class Season : Event
    {
        public DateTime Year { get; set; }
    }
}
