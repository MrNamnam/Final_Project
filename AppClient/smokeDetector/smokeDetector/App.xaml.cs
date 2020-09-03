using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using smokeDetector.views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace smokeDetector
{
    public partial class App : Application
    {
        public App()
        {
             InitializeComponent();
            bool isLoggedIn =  Current.Properties.ContainsKey("IsLoggedIn") ? Convert.ToBoolean(Current.Properties["IsLoggedIn"]) : false;


            if (!isLoggedIn)
            {
                MainPage = new NavigationPage(new MainPage())
                {
                //BarBackgroundColor = Color.FromHex("#ffffff")
                };
            }
            else
            {               
                UsersListPage.Email = Current.Properties.ContainsKey("Email") ? Convert.ToString(Current.Properti‌​es["Email"]) : "";
                MainPage = new NavigationPage(new UsersListPage());
            }
        }

        public static async void NavigatiPage(Page name)
        {
            Application.Current.MainPage = new NavigationPage(new UsersListPage());
            // new NavigationPage(new UsersListPage());
            //Application.Current.MainPage = navPage;
            await name.Navigation.PushAsync(new UsersListPage());
        }
        public static void MainPageList()
        {
            //MainPage = new NavigationPage(new )
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
          
        }

        protected override void OnResume()
        {

            // Handle when your app resumes
        }

        internal static async Task NavigatiPageAsync(Page name)
        {
            UsersListPage page = new UsersListPage();
             Application.Current.MainPage = new NavigationPage(new UsersListPage());
            // new NavigationPage(new UsersListPage());
            //Application.Current.MainPage = navPage;
            await name.Navigation.PushAsync(page);
        }
    }
  
}
