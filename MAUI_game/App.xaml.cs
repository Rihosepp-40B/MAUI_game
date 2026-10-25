using MAUI_game.Services;

namespace MAUI_game
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            LanguageService.Initialize();
            ThemeService.Initialize();
        }

        //Ilma selleta viskab exceptioni sisse (ilmselt kuna asjad käivad läbi AppShelli ikka)
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}