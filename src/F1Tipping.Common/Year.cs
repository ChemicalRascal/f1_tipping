namespace F1Tipping.Common;

public readonly struct Year
    : IEquatable<Year>, IEquatable<DateTime>, IEquatable<int>,
    IComparable<Year>, IComparable<DateTime>, IComparable<int>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="year"></param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     When <see cref="year"/> is not within the range
    ///     from <value>1</value> to <value>9999</value>.
    /// </exception>
    public Year(int year)
    {
        // same limits as DateTime 
        // be careful when changing this values, because it might break
        // conversion from and to DateTime 
        var min = 1;
        var max = 9999;

        if (year < min || year > max)
        {
            var message = string.Format(
                "Year must be between {0} and {1}.", min, max);
            throw new ArgumentOutOfRangeException("year", year, message);
        }

        _value = year;
    }

    private readonly int _value;

    public bool Equals(Year other) => _value == other._value;
    public bool Equals(DateTime other) => _value == other.Year;
    public bool Equals(int other) => _value == other;

    public int CompareTo(Year other) => _value.CompareTo(other._value);
    public int CompareTo(DateTime other) => _value.CompareTo(other.Year);
    public int CompareTo(int other) => _value.CompareTo(other);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        switch (obj)
        {
            case Year year: return Equals(year);
            case DateTime year: return Equals(year);
            case int year: return Equals(year);
        }
        return false;
    }

    public static Year MinValue { get => new Year(DateTime.MinValue.Year); }
    public static Year MaxValue { get => new Year(DateTime.MaxValue.Year); }

    public override int GetHashCode() => _value;

    public static bool operator ==(Year left, Year right)
        => left.Equals(right);
    public static bool operator !=(Year left, Year right)
        => !left.Equals(right);

    public static bool operator <(Year left, DateTime right)
        => left.CompareTo(right) < 0;
    public static bool operator >(Year left, DateTime right)
        => left.CompareTo(right) > 0;
    public static bool operator ==(Year left, DateTime right)
        => left.CompareTo(right) == 0;
    public static bool operator !=(Year left, DateTime right)
        => left.CompareTo(right) != 0;

    public static bool operator <(Year left, DateTimeOffset right)
        => left.CompareTo(right.UtcDateTime) < 0;
    public static bool operator >(Year left, DateTimeOffset right)
        => left.CompareTo(right.UtcDateTime) > 0;
    public static bool operator ==(Year left, DateTimeOffset right)
        => left.CompareTo(right.UtcDateTime) == 0;
    public static bool operator !=(Year left, DateTimeOffset right)
        => left.CompareTo(right.UtcDateTime) != 0;

    public override string ToString() => _value.ToString();

    public string ToString(IFormatProvider formatProvider)
        => _value.ToString(formatProvider);

    public string ToString(string format) => _value.ToString(format);

    public string ToString(string format, IFormatProvider formatProvider)
        => _value.ToString(format, formatProvider);

    public DateTime ToDateTime() => new DateTime(_value, 1, 1);

    public int ToInt() => _value;

    public static implicit operator DateTime(Year year)
        => year.ToDateTime();

    public static explicit operator Year(DateTime dateTime)
        => new Year(dateTime.Year);

    public static explicit operator int(Year year) => year._value;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     When <see cref="year"/> is not within the range from
    ///     <value>1</value> to <value>9999</value>.
    /// </exception>
    public static explicit operator Year(int year) => new Year(year);
}