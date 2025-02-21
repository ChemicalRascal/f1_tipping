namespace F1Tipping.Pages.Admin.Users
{
    public record UserView(Guid Id, string? Email);
    public record UserRoleView(Guid UserId, string Role, bool UserInRole);
}
