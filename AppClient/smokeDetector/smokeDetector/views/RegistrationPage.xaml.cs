using smokeDetector.Helper;
using System;
using System.Diagnostics;
using System.Net;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace smokeDetector.views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegistrationPage : ContentPage
    {
        string RegisterUrl = "https://smokingdetectorfunction.azurewebsites.net/api/Register/";

        public RegistrationPage()
        {

            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
            emailEntry.ReturnCommand = new Command(() => userNameEntry.Focus());
            userNameEntry.ReturnCommand = new Command(() => passwordEntry.Focus());
            passwordEntry.ReturnCommand = new Command(() => confirmpasswordEntry.Focus());
            confirmpasswordEntry.ReturnCommand = new Command(() => favoriteColor.Focus());
            favoriteColor.ReturnCommand = new Command(() => phoneEntry.Focus());
            
        }

        private async void SignupValidation_ButtonClicked(object sender, EventArgs e)
        {

            // validate inputs
            if ((string.IsNullOrWhiteSpace(userNameEntry.Text)) || (string.IsNullOrWhiteSpace(emailEntry.Text)) ||
                (string.IsNullOrWhiteSpace(passwordEntry.Text)) || (string.IsNullOrWhiteSpace(phoneEntry.Text)) ||
                (string.IsNullOrEmpty(userNameEntry.Text)) || (string.IsNullOrEmpty(emailEntry.Text)) ||
                (string.IsNullOrEmpty(passwordEntry.Text)) || (string.IsNullOrEmpty(phoneEntry.Text)) || (string.IsNullOrEmpty(favoriteColor.Text)))

            {
                await DisplayAlert("Enter Data", "Enter Valid Data", "OK");
            }
            else if (emailEntry.TextColor == Color.Red)
            {
                emailEntry.Text = string.Empty;
                phoneWarLabel.Text = "Enter Valid Email";
                phoneWarLabel.TextColor = Color.IndianRed;
                phoneWarLabel.IsVisible = true;
            }
            else if (!string.Equals(passwordEntry.Text, confirmpasswordEntry.Text))
            {
                warningLabel.Text = "Enter Same Password";
                passwordEntry.Text = string.Empty;
                confirmpasswordEntry.Text = string.Empty;
                warningLabel.TextColor = Color.IndianRed;
                warningLabel.IsVisible = true;
            }
            else if (phoneEntry.Text.Length < 10)
            {
                phoneEntry.Text = string.Empty;
                phoneWarLabel.Text = "Enter 10 digit Number";
                phoneWarLabel.TextColor = Color.IndianRed;
                phoneWarLabel.IsVisible = true;
            }
            else
            {
                // register user
                try
                {
                    string searchApiUrl = RegisterUrl + emailEntry.Text + "/" + passwordEntry.Text + "/" + userNameEntry.Text + "/" +
                        phoneEntry.Text.ToString() + "/" + favoriteColor.Text;
                    var results = Utilities.ExecuteWebRequest(searchApiUrl, method: "GET");
                    var retrunvalue = results.ResponseContents.ToString();
                    if (results.StatusCode != HttpStatusCode.OK)
                    {
                        await DisplayAlert("User Add", retrunvalue, "OK");
                        warningLabel.IsVisible = false;
                        emailEntry.Text = string.Empty;
                        userNameEntry.Text = string.Empty;
                        passwordEntry.Text = string.Empty;
                        confirmpasswordEntry.Text = string.Empty;
                        favoriteColor.Text = string.Empty;
                        phoneEntry.Text = string.Empty;
                    }


                    else
                    {
                        await DisplayAlert("User Add", retrunvalue, "OK");

                        await Navigation.PushAsync(new LoginPage());

                    }
                }

                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        private async void login_ClickedEvent(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }

    }
}