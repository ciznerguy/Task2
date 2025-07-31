using System.Net.Http.Json;
using MauiModels.Models;

namespace LoginApp.Views;

public partial class UsersListPage : ContentPage
{
    private readonly HttpClient httpClient = new()
    {
        BaseAddress = new Uri("http://10.0.2.2:5013/") // ✅ נדרש באמולטור אנדרואיד
    };

    private User? _selectedUser;

    public UsersListPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUsersFromApi();
    }

    private async Task LoadUsersFromApi()
    {
        try
        {
            var users = await httpClient.GetFromJsonAsync<List<User>>("api/users");

            if (users is not null)
            {
                Console.WriteLine($"✅ נטענו {users.Count} משתמשים");
                UsersCollectionView.ItemsSource = users;
                EmptyLabel.IsVisible = users.Count == 0;
            }
            else
            {
                Console.WriteLine("⚠️ לא התקבלו משתמשים");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ שגיאת HttpClient: {ex.Message}");
            await DisplayAlert("שגיאה", "נכשלה טעינת המשתמשים מהשרת", "סגור");
        }
    }

    private void UsersCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedUser = e.CurrentSelection.FirstOrDefault() as User;
    }

    private async void DeleteSelectedUser_Clicked(object sender, EventArgs e)
    {
        if (_selectedUser is null)
        {
            await DisplayAlert("שגיאה", "חובה לבחור משתמש לפני מחיקה", "סגור");
            return;
        }

        bool confirm = await DisplayAlert(
            "אישור מחיקה",
            $"האם למחוק את המשתמש {_selectedUser.FullName}?",
            "מחק", "בטל");

        if (!confirm)
            return;

        try
        {
            var response = await httpClient.DeleteAsync($"api/users/{_selectedUser.Id}");

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("הצלחה", "המשתמש נמחק", "סגור");
                await LoadUsersFromApi();
            }
            else
            {
                await DisplayAlert("שגיאה", "המחיקה נכשלה", "סגור");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ שגיאת מחיקה: {ex.Message}");
            await DisplayAlert("שגיאה", "שגיאה במחיקת המשתמש", "סגור");
        }

        _selectedUser = null;
        UsersCollectionView.SelectedItem = null;
    }
}
