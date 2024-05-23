using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TBmobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
            /*#FF0070B5
            #FF383838
            #1e1e1e*/
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
