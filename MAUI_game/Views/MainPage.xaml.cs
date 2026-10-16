using MAUI_game.Services;

namespace MAUI_game.Views
{
    public partial class MainPage : ContentPage
    {
        private double _cachedCutoutWidthDp = 0; // Siia salvestame kaamera riba mõõdetud laiuse
        public MainPage()
        {
            InitializeComponent();
            DeviceDisplay.MainDisplayInfoChanged += OnDeviceDisplayInfoChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Mõõdame ja salvestame laiuse
            _cachedCutoutWidthDp = await DisplayCutoutHelper.CheckAndMeasureCutout();

            // Rakendame paddingu
            ApplyPaddingBasedOnRotation(DeviceDisplay.Current.MainDisplayInfo.Rotation);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            DeviceDisplay.MainDisplayInfoChanged -= OnDeviceDisplayInfoChanged;
        }

        private void OnDeviceDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
        {
            ApplyPaddingBasedOnRotation(e.DisplayInfo.Rotation);
        }

        private void ApplyPaddingBasedOnRotation(DisplayRotation rotation)
        // rakendame kaamera riba vastaskülje kuva
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Küsime abiklassilt arvutatud Thickness väärtuse
                RootGrid.Padding = DisplayCutoutHelper.GetPaddingForRotation(rotation, _cachedCutoutWidthDp);
            });
        }

    }
}