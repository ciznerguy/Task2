using System.Net.Http.Json;
using System.Text.Json;
using MauiModels.Models;
using LoginApp.Services;
using LoginApp.Views;

namespace LoginApp.Views;

public partial class LoginPage : ContentPage
{
    private readonly HttpClient httpClient = new()
    {
        BaseAddress = new Uri("http://10.0.2.2:5013/")
    };

    private readonly IUserSessionService sessionService;

    public LoginPage(IUserSessionService sessionService)
    {
        InitializeComponent();
        this.sessionService = sessionService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // ✅ שלב 1: הצגת האינדיקטור ונטרול הכפתור למניעת לחיצות כפולות
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        LoginButton.IsEnabled = false;

        try
        {
            ErrorLabel.IsVisible = false;

            var username = UsernameEntry.Text?.Trim();
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorLabel.Text = "נא להזין שם משתמש וסיסמה";
                ErrorLabel.IsVisible = true;
                return; // הבלוק finally עדיין יתבצע
            }

            var loginDto = new UserLoginRequest
            {
                Username = username,
                Password = password
            };

            try
            {
                var response = await httpClient.PostAsJsonAsync("api/users/login", loginDto);
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"🔁 תגובת שרת: {responseContent}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ErrorLabel.Text = "שם משתמש או סיסמה שגויים";
                    ErrorLabel.IsVisible = true;
                }
                else if (!response.IsSuccessStatusCode)
                {
                    ErrorLabel.Text = "שגיאה בתקשורת עם השרת";
                    ErrorLabel.IsVisible = true;
                }
                else
                {
                    var user = JsonSerializer.Deserialize<User>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (user is not null)
                    {
                        sessionService.SaveUserInfo(user.Username, user.Role == 1 ? "Admin" : "User");
                        await DisplayAlert("התחברות", "התחברת בהצלחה", "אישור");

                        // ✅ שלב 2: הוספת השהיה של 2 שניות לאחר התחברות מוצלחת
                        await Task.Delay(2000);

                        Application.Current.MainPage = new AppShell(sessionService);
                        await Shell.Current.GoToAsync(user.Role == 1 ? "//UsersListPage" : "//MainPage");
                    }
                    else
                    {
                        ErrorLabel.Text = "שגיאה בקריאת נתוני המשתמש";
                        ErrorLabel.IsVisible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HttpClient Error: {ex.Message}");
                ErrorLabel.Text = $"שגיאה: {ex.Message}";
                ErrorLabel.IsVisible = true;
            }
        }
        finally
        {
            // ✅ שלב 3: הסתרת האינדיקטור והחזרת הכפתור למצב פעיל
            // הבלוק הזה יתבצע תמיד, גם אם הייתה שגיאה
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            LoginButton.IsEnabled = true;
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage(sessionService));
    }
}