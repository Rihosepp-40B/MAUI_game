namespace MAUI_game.Services
{
    public static class DisplayCutoutHelper
    // Abimeetod, kaamerariba laiuse leidmiseks ning sama suguse riba kuvamiseks vastas küljel
    {
        public static async Task<double> CheckAndMeasureCutout()
        // Kontrollib ja tagastab vkaamera vastas külje jaoks riba
        {
            double cutoutWidth = 0;

#if ANDROID
            await Task.Delay(100); // Väike ooteaeg, et aken jõuaks initsialiseeruda
            try
            {
                var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
                if (activity?.Window?.DecorView?.RootWindowInsets != null)
                {
                    var decorView = activity.Window.DecorView;
                    var insets = AndroidX.Core.View.WindowInsetsCompat.ToWindowInsetsCompat(
                        decorView.RootWindowInsets, decorView);

                    if (insets != null)
                    {
                        int left = insets.GetInsets(AndroidX.Core.View.WindowInsetsCompat.Type.DisplayCutout()).Left;
                        int right = insets.GetInsets(AndroidX.Core.View.WindowInsetsCompat.Type.DisplayCutout()).Right;

                        if (left == 0 && right == 0)
                        {
                            left = insets.GetInsets(AndroidX.Core.View.WindowInsetsCompat.Type.SystemBars()).Left;
                            right = insets.GetInsets(AndroidX.Core.View.WindowInsetsCompat.Type.SystemBars()).Right;
                        }

                        // Kutsume välja funktsiooni ja salvestame tulemuse mällu
                        cutoutWidth = CameraBarWidth(left, right);
                    }
                }
            }
            catch
            {
                cutoutWidth = CameraBarWidth(40, 40); // Varupuhver
            }
#else
            cutoutWidth = CameraBarWidth(40, 40);
#endif

            return cutoutWidth;
        }

        internal static double CameraBarWidth(int left, int right)
        // Tagastab kaamerariba laiuse
        {
            var detectedPixels = Math.Max(left, right);
            var density = DeviceDisplay.MainDisplayInfo.Density;
            if (density > 0 && detectedPixels > 0)
            {
                // Salvestame reaalse mõõdetud laiuse (dp-des) mällu!
                return (detectedPixels / density);
            }

            return 0;
        }

        public static Thickness GetPaddingForRotation(DisplayRotation rotation, double cutoutWidth)
        // Tagastab vastavalt ekraani rotatsioonile kaamerariba laiuse sobivale küljele
        {
            double leftDp = 0;
            double rightDp = 0;

            // Suuname paddingu vastavalt ekraani rotatsioonile vastaspoolele
            if (rotation == DisplayRotation.Rotation270)
            {
                leftDp = cutoutWidth;
            }
            else
            {
                rightDp = cutoutWidth;
            }

            return new Thickness(leftDp, 0, rightDp, 0);
        }
    }
}
