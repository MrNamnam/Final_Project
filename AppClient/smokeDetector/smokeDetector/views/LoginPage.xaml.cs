using Newtonsoft.Json;
using smokeDetector.Helper;
using System;
using System.Net;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace smokeDetector.views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        string SignInUrl = "https://smokingdetectorfunction.azurewebsites.net/api/SignIn/";
        string CheckColorUrl = "https://smokingdetectorfunction.azurewebsites.net/api/CheckColor/";
        string UpdatePasswordUrl = "https://smokingdetectorfunction.azurewebsites.net/api/UpdatePassword/";
        string Email = "";

        public LoginPage() 
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
            userNameEntry.ReturnCommand = new Command(() => passwordEntry.Focus());
            email.ReturnCommand = new Command(() => favoriteColor.Focus());
            firstPassword.ReturnCommand = new Command(() => secondPassword.Focus());
            var forgetpassword_tap = new TapGestureRecognizer();
            forgetpassword_tap.Tapped += Forgetpassword_tap_Tapped;
            forgetLabel.GestureRecognizers.Add(forgetpassword_tap);
        }

        private async void Forgetpassword_tap_Tapped(object sender, EventArgs e)
        {
              popupLoadingView.IsVisible = true;
        }

        private async void userIdCheckEvent(object sender, EventArgs e)
        {
             if ((string.IsNullOrWhiteSpace(email.Text)) || (string.IsNullOrWhiteSpace(email.Text)))
             {             
                 await DisplayAlert("Alert", "Enter Mail Id","OK"); 
             }
             else if ((string.IsNullOrWhiteSpace(favoriteColor.Text)))
             {
                await DisplayAlert("Alert", "Enter Your Favorite Color", "OK");
             }
             else
             {
                string searchApiUrl = CheckColorUrl + email.Text + "/" + favoriteColor.Text;
                var results = Utilities.ExecuteWebRequest(searchApiUrl, method: "GET");
                var retrunvalue = results.ResponseContents.ToString();
                if (results.StatusCode != HttpStatusCode.OK)
                {
                    await DisplayAlert("Alert", retrunvalue, "OK");
                }
                else 
                {
                     Email = email.Text;
                     popupLoadingView.IsVisible = false;
                     passwordView.IsVisible = true;
                 }                 
             }
        }

        private async void Password_ClickedEvent(object sender, EventArgs e)
        {
            if (!string.Equals(firstPassword.Text, secondPassword.Text))
            {
                warningLabel.Text = "Enter Same Password";
                warningLabel.TextColor = Color.IndianRed;
                warningLabel.IsVisible = true;
            }
            else if ((string.IsNullOrWhiteSpace(firstPassword.Text)) || (string.IsNullOrWhiteSpace(secondPassword.Text)))
            {
                await DisplayAlert("Alert", " Enter Password", "OK");
            }
            else
            {
                try
                {
                    string searchApiUrl = UpdatePasswordUrl + Email + "/" + firstPassword.Text;
                    var results = Utilities.ExecuteWebRequest(searchApiUrl, method: "GET");
                    var retrunvalue = results.ResponseContents.ToString(); 
                    passwordView.IsVisible = false;
                    await DisplayAlert("Password Updated","User Data updated","OK");
                    Email = "";
                }
                catch (Exception)
                {
                     throw;
                }
            }
        }

        // Validate inputs and login 
        private async void LoginValidation_ButtonClicked(object sender, EventArgs e)
        {

            if (userNameEntry.Text != null && passwordEntry.Text != null)
            {
                string searchApiUrl = SignInUrl + userNameEntry.Text + "/" + passwordEntry.Text;
                var results = Utilities.ExecuteWebRequest(searchApiUrl, method: "GET");
                var retrunvalue = results.ResponseContents.ToString();
                if (results.StatusCode != HttpStatusCode.OK)
                {
                    popupLoadingView.IsVisible = false;
                    await DisplayAlert("Login Failed", retrunvalue, "OK");

                }
                else
                {
                    UsersListPage.Email = userNameEntry.Text;
                    popupLoadingView.IsVisible = false;
                    await App.NavigatiPageAsync(loginPage);

                    //Save App param
                    if (!App.Current.Properties.ContainsKey("IsLoggedIn"))
                    {
                        Application.Current.Properties.Add("IsLoggedIn", Boolean.TrueString);

                    }
                    else
                    {
                        Application.Current.Properties["IsLoggedIn"] = Boolean.TrueString;

                    }

                    if (!App.Current.Properties.ContainsKey("Email"))
                    {
                        Application.Current.Properties.Add("Email", UsersListPage.Email);
                    }
                    else
                    {
                        Application.Current.Properties["Email"] = UsersListPage.Email;
                    }
                    await App.Current.SavePropertiesAsync();
                }
            }
            else
            {
                popupLoadingView.IsVisible = false;
                await DisplayAlert("He He", "Enter User Name and Password Please", "OK");
            }
        }
    }
}