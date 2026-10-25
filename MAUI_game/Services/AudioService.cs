using Plugin.Maui.Audio;
using MAUI_game.Models;

namespace MAUI_game.Services
{
    public class AudioService
    {
        private readonly IAudioManager _audioManager;

        private static readonly Dictionary<string, byte[]> _soundCache = new Dictionary<string, byte[]>();

        public AudioService()
        {
            _audioManager = AudioManager.Current;
        }

        // Meetod helifaili esitamiseks
        public async Task PlaySoundAsync(string fileName)
        {
            if (!GameSettingsModel.IsSoundEnabled) return;

            if (string.IsNullOrEmpty(fileName)) return;

            try
            {
                // Kui heli pole veel mälus, laeme selle sinna
                if (!_soundCache.ContainsKey(fileName))
                {
                    using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    _soundCache[fileName] = memoryStream.ToArray();
                }

                // Loome mälus olevatest baitidest uue striimi
                var cacheStream = new MemoryStream(_soundCache[fileName]);
                var player = _audioManager.CreatePlayer(cacheStream);
                player.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Heli esitamine ebaõnnestus: {ex.Message}");
            }
        }
    }
}
