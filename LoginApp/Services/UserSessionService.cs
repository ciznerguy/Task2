using Microsoft.Maui.Storage;

namespace LoginApp.Services;

public interface IUserSessionService
{
    void SaveUserInfo(string username, string role);
    (string Username, string Role)? GetUserInfo();
    void ClearUserInfo();
}

public class UserSessionService : IUserSessionService
{
    public void SaveUserInfo(string username, string role)
    {
        Preferences.Set("Username", username);
        Preferences.Set("UserRole", role);
    }

    public (string Username, string Role)? GetUserInfo()
    {
        var username = Preferences.Get("Username", null);
        var role = Preferences.Get("UserRole", null);

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(role))
            return null;

        return (username, role);
    }

    public void ClearUserInfo()
    {
        Preferences.Remove("Username");
        Preferences.Remove("UserRole");
    }
}
