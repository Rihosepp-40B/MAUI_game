using Android.App;
using Android.Content.PM;
using Android.OS;

namespace MAUI_game
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //// Sunnib rakenduse ainult landscape vaatesse, sunnib ainult ühele küljele
            //RequestedOrientation = ScreenOrientation.Landscape;

            // See lubab landscape mõlemal küljel
            RequestedOrientation = ScreenOrientation.SensorLandscape;

            // See kaotab androidis ära statusbar'i
            #if ANDROID
                var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
                activity?.Window?.AddFlags(Android.Views.WindowManagerFlags.Fullscreen);
                activity?.Window?.ClearFlags(Android.Views.WindowManagerFlags.ForceNotFullscreen);
            #endif
        }
    }
}