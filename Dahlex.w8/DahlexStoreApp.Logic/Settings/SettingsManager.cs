using Dahlex.Logic.Contracts;
using Dahlex.Logic.Logger;
using Windows.Storage;

namespace Dahlex.Logic.Settings
{
    public class SettingsManager : ISettingsManager
    {
        private static bool _hasAlreadyRun;
        private IntSize _canvasSize;

        public SettingsManager(IntSize canvasSize)
        {
            _canvasSize = canvasSize;
        }

        /// <summary>Will return false only the first time a user ever runs this.
        /// Everytime thereafter, a placeholder file will have been written to disk
        /// and will trigger a value of true.</summary>
        public static bool IsFirstRun()
        {
            if (_hasAlreadyRun)
            {
                return false;
            }

            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey("LastStartedVersion"))
            {
                _hasAlreadyRun = true;
                return false;
            }
            else
            {
                settings.Values["LastStartedVersion"] = Windows.ApplicationModel.Package.Current.Id.Version.ToString();
                _hasAlreadyRun = true;
                return true;
            }
        }


        public static int MaxLevelIndicator
        { get { return 100; } }

        public static int MinLevelIndicator
        { get { return 0; } }

        public GameSettings LoadLocalSettings()
        {
            var settings = new GameSettings(_canvasSize);
            var localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey("PlayerName"))
            {
                settings.PlayerName = localSettings.Values["PlayerName"].ToString();
                settings.LessSound = (bool)localSettings.Values["LessSound"];

            }
            else
            {
                settings.PlayerName = "Dr. D";
                settings.LessSound = true;
                GameLogger.AddLineToLog(string.Format("No settings found"));
            }

            return settings;
        }

        public void SaveLocalSettings(GameSettings settings)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["PlayerName"] = settings.PlayerName;
            localSettings.Values["LessSound"] = settings.LessSound;
        }
    }
}