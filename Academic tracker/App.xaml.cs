using Academic_tracker.Pages;
using Academic_tracker.Services;

namespace Academic_tracker
{
    public partial class App : Application
    {
        public App(DBServices db)
        {
            InitializeComponent();

            // Check if a user is already logged in from a previous session
            int savedUserID = Preferences.Get("loggeInUserID", -1);
            
            if (savedUserID != -1)
            {
                // Skip login screen - go straight to theirboard
                MainPage = new NavigationPage(new Dashboard(db, savedUserID));
            }
            else
            {
                MainPage = new NavigationPage(new LoginPage(db));
            }
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