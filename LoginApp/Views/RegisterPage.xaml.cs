// --- Importing Required Libraries ---
// Allows sending HTTP requests and handling responses, including working with JSON.
using System.Net.Http.Json;
// Imports the application's models (e.g., the 'User' model).
using MauiModels.Models;
// Imports custom services, such as user session management.
using LoginApp.Services;
// Provides access to useful methods for strings and collections, like 'Any'.
using System.Linq;
// Allows working with regular expressions (Regex) for input validation (like email).
using System.Text.RegularExpressions;

// Defines the namespace for the application's View files.
namespace LoginApp.Views;

// Defines the code-behind class for the registration page.
// It's a 'partial' class because its UI is defined in a separate XAML file.
public partial class RegisterPage : ContentPage
{
    // --- Class Members and Properties ---

    // Creates an HttpClient object to be used for sending requests to the API server.
    private readonly HttpClient httpClient = new()
    {
        // Sets the base address of the server.
        // The address 10.0.2.2 is a special IP that allows the Android emulator
        // to access the localhost of the host computer.
        BaseAddress = new Uri("http://10.0.2.2:5013/")
    };

    // A variable to hold the user session management service,
    // which is received via dependency injection.
    private readonly IUserSessionService sessionService;

    // --- Constructor ---
    // This method is called when an instance of the page is created.
    public RegisterPage(IUserSessionService sessionService)
    {
        // Initializes the UI components defined in the XAML file.
        InitializeComponent();
        // Assigns the injected session service to the local variable.
        this.sessionService = sessionService;
        // Sets the initial age label based on the default date.
        UpdateAgeLabel(BirthDatePicker.Date);
    }

    // --- Page Lifecycle Methods ---

    // This method is called every time the page appears on the screen.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Clears the form to ensure all fields are reset.
        ClearForm();
    }

    // --- Helper Methods ---

    // Resets all input fields on the form to their default state.
    private void ClearForm()
    {
        UsernameEntry.Text = string.Empty;
        FullNameEntry.Text = string.Empty;
        EmailEntry.Text = string.Empty;
        PasswordEntry.Text = string.Empty;
        ConfirmPasswordEntry.Text = string.Empty;
        BirthDatePicker.Date = DateTime.Today;
        UpdateAgeLabel(DateTime.Today);
        ErrorLabel.IsVisible = false; // Hide any previous error messages.
    }

    // Calculates the age based on a given birth date.
    private int CalculateAge(DateTime birthDate)
    {
        // Calculates the total days and divides by 365.25 to account for leap years.
        return (int)((DateTime.Today - birthDate).TotalDays / 365.25);
    }

    // Updates the AgeLabel text to display the calculated age.
    private void UpdateAgeLabel(DateTime birthDate)
    {
        AgeLabel.Text = $"Age: {CalculateAge(birthDate)}";
    }

    // Validates the password against a set of rules.
    private string? ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return "Password cannot be empty.";
        if (!password.Any(char.IsUpper))
            return "Password must contain at least one uppercase letter.";
        if (!password.Any(char.IsDigit))
            return "Password must contain at least one number.";

        // Returns null if the password is valid.
        return null;
    }

    // --- Event Handlers for Real-time Validation ---

    // Event handler that triggers when the user selects a new date in the date picker.
    private void BirthDatePicker_DateSelected(object sender, DateChangedEventArgs e)
    {
        // Updates the age label with the newly selected date.
        UpdateAgeLabel(e.NewDate);
    }

    // Event handler that triggers when the Username entry loses focus.
    private void Username_Unfocused(object sender, FocusEventArgs e)
    {
        string username = UsernameEntry.Text ?? "";
        ErrorLabel.IsVisible = false; // Reset previous error message.

        if (string.IsNullOrWhiteSpace(username)) return;

        if (username.Contains(" "))
        {
            ErrorLabel.Text = "Username cannot contain spaces.";
            ErrorLabel.IsVisible = true;
        }
        else if (char.IsDigit(username[0]))
        {
            ErrorLabel.Text = "Username cannot start with a digit.";
            ErrorLabel.IsVisible = true;
        }
    }

    // Event handler that triggers when the Email entry loses focus.
    private void Email_Unfocused(object sender, FocusEventArgs e)
    {
        string email = EmailEntry.Text ?? "";
        ErrorLabel.IsVisible = false; // Reset previous error message.

        if (string.IsNullOrWhiteSpace(email)) return;

        // Uses a regular expression to validate the email format.
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(email, emailPattern))
        {
            ErrorLabel.Text = "Invalid email address format.";
            ErrorLabel.IsVisible = true;
        }
    }

    // --- Main Event Handlers for Form Actions ---

    // Event handler for the "Register" button click.
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        // Get and trim all input values from the form.
        string fullName = FullNameEntry.Text?.Trim() ?? "";
        string username = UsernameEntry.Text?.Trim() ?? "";
        string email = EmailEntry.Text?.Trim() ?? "";
        string password = PasswordEntry.Text ?? "";
        string confirmPassword = ConfirmPasswordEntry.Text ?? "";
        DateTime birthDate = BirthDatePicker.Date;

        // --- Input Validation ---
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "All fields must be filled.", "Close");
            return;
        }

        if (password != confirmPassword)
        {
            await DisplayAlert("Error", "Passwords do not match.", "Close");
            return;
        }

        string? passwordError = ValidatePassword(password);
        if (passwordError != null)
        {
            await DisplayAlert("Password Error", passwordError, "Close");
            return;
        }

        int age = CalculateAge(birthDate);
        if (age < 18)
        {
            await DisplayAlert("Age Error", "Registration is only allowed for users 18 and older.", "Close");
            return;
        }

        // Create a new User object with the validated data.
        var user = new User
        {
            FullName = fullName,
            Username = username,
            Email = email,
            Password = password,
            BirthDate = birthDate,
            Age = age,
            Role = 0 // Default role for a new user.
        };

        // --- API Call to Register User ---
        try
        {
            // Send the user object to the API endpoint as JSON.
            var response = await httpClient.PostAsJsonAsync("api/users", user);

            if (response.IsSuccessStatusCode)
            {
                // If registration is successful, show a success message and navigate back.
                await DisplayAlert("Success", "Registration completed successfully!", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                // If the server returns an error, notify the user.
                await DisplayAlert("Error", "Registration failed. Please try again.", "Close");
            }
        }
        catch (Exception ex)
        {
            // Catch exceptions related to the HTTP request (e.g., no network).
            Console.WriteLine($"HttpClient Error: {ex.Message}");
            await DisplayAlert("Error", "An error occurred while contacting the server.", "Close");
        }
    }

    // Event handler for the "Sign In" tap gesture.
    private async void OnSignInTapped(object sender, EventArgs e)
    {
        // Navigates the user back to the previous page (presumably the login page).
        await Navigation.PopAsync();
    }
}