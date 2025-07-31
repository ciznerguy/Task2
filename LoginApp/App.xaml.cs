using LoginApp.Services;
using LoginApp.Views;

namespace LoginApp
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            var session = serviceProvider.GetRequiredService<IUserSessionService>();

            // ✅ איפוס תמידי של פרטי המשתמש – בכל הרצה
            session.ClearUserInfo();

            var isLoggedIn = session.GetUserInfo() != null;

            if (isLoggedIn)
            {
                // משתמש כבר מחובר → טען את Shell
                MainPage = serviceProvider.GetRequiredService<AppShell>();
            }
            else
            {
                // אין משתמש מחובר → טען את דף Login בלבד
                MainPage = new NavigationPage(new LoginPage(session));
            }
        }
    }
}
