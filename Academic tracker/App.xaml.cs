using Academic_tracker.Pages;
using Academic_tracker.Services;

namespace Academic_tracker
{
    public partial class App : Application
    {
        public App(DBServices db)
        {
            InitializeComponent();

            var loginPage = new LoginPage(db);
            MainPage = new NavigationPage(loginPage);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);

            window.Width = 400;
            window.Height = 750;

            return window;
        }
    }
}