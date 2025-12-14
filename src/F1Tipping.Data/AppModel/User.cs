using Microsoft.AspNetCore.Identity;

namespace F1Tipping.Data.AppModel;

public class User : IdentityUser<Guid>
{
    public UserSettings Settings { get; init; } = new();
    public TemporalUserData TemporalData { get; init; } = new();

    public User(string userName) : base(userName)
    { }

    public User() : base()
    { }

}
