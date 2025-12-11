using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EsspronAlcoholTester.Services;
using EsspronAlcoholTester.Models;

namespace EsspronAlcoholTester.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ISupabaseService _supabaseService;
        
        public event EventHandler? LoginSuccessful;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _rememberMe;

        [ObservableProperty]
        private bool _isSignUpMode;

        [ObservableProperty]
        private string _fullName = string.Empty;

        [ObservableProperty]
        private string _companyName = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        public LoginViewModel()
        {
            _supabaseService = App.ServiceProvider.GetService(typeof(ISupabaseService)) as ISupabaseService 
                ?? throw new InvalidOperationException("SupabaseService not found");
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter your email and password.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var success = await _supabaseService.SignInAsync(Email, Password);
                
                if (success)
                {
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorMessage = "Invalid email or password. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SignUpAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please fill in all required fields.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }

            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(CompanyName))
            {
                ErrorMessage = "Please enter your full name and company name.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var success = await _supabaseService.SignUpAsync(Email, Password, FullName, CompanyName);
                
                if (success)
                {
                    // Auto-login after successful signup
                    var loginSuccess = await _supabaseService.SignInAsync(Email, Password);
                    if (loginSuccess)
                    {
                        LoginSuccessful?.Invoke(this, EventArgs.Empty);
                    }
                }
                else
                {
                    ErrorMessage = "Sign up failed. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Sign up failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ToggleMode()
        {
            IsSignUpMode = !IsSignUpMode;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private void ForgotPassword()
        {
            // TODO: Implement password reset
            ErrorMessage = "Password reset functionality coming soon.";
        }
    }
}
