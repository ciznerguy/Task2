using LoginApp.Services;

namespace LoginApp;
using LoginApp.Views;

public partial class AppShell : Shell
{
    private readonly IUserSessionService session;

    public static AppShell Current { get; private set; }

    public AppShell(IUserSessionService session)
    {
        InitializeComponent();
        this.session = session;

        AppShell.Current = this; 

        SetUserTitle();
    }

    public void SetUserTitle()
    {
        var userInfo = session.GetUserInfo();
        if (userInfo != null)
        {
            var (username, role) = userInfo.Value;
            UserTitleLabel.Text = $"שלום {username} ({role})";
        }
        else
        {
            UserTitleLabel.Text = "שלום אורח"; // ✅ לא ריק
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        session.ClearUserInfo();
        SetUserTitle();

        // החלף את ה־MainPage למסך Login
        Application.Current.MainPage = new NavigationPage(new LoginPage(session));
    }
}
