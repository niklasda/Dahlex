using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using Dahlex.Logic.Contracts;
using Dahlex.Logic.Logger;

namespace Dahlex.Logic.Settings
{
    public class SettingsManager : ISettingsManager
    {
        private static bool _hasAlreadyRun;
        private const string LandingBitFileName = "LastStartedVersion.txt";

        /// <summary>Will return false only the first time a user ever runs this.
        /// Everytime thereafter, a placeholder file will have been written to disk
        /// and will trigger a value of true.</summary>
        public static bool IsFirstRun()
        {
            if (_hasAlreadyRun)
            {
                return false;
            }

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(LandingBitFileName))
                {
                    // just write a placeholder file one byte long so we know they've landed before
                    using (var stream = store.OpenFile(LandingBitFileName, FileMode.Create))
                    {
                        stream.Write(new[] { (byte)'1', (byte)'.', (byte)'1' }, 0, 3);
                    }
                    return true;
                }

                _hasAlreadyRun = true;
                return false;
            }
        }

        public static int MaxLevelIndicator
        { get { return 100; } }

        public static int MinLevelIndicator
        { get { return 0; } }

        public GameSettings LoadLocalSettings()
        {
            // try IsolatedStorageSettings

            var settings = new GameSettings();
            using (IsolatedStorageFile root = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var stream = new IsolatedStorageFileStream("dahlexsettings.xml", FileMode.OpenOrCreate, root))
                {
                    if (stream.Length > 0)
                    {
                        var serializer = new DataContractSerializer(typeof(GameSettings));
                        try
                        {
                            settings = (GameSettings)serializer.ReadObject(stream);
                        }
                        catch (Exception ex)
                        {
                            GameLogger.AddLineToLog(string.Format("Settings failed to load: {0}", ex.Message));
                        }
                    }
                    else // no settings file, use defaults
                    {
                        settings.PlayerName = "Dr. D";
                        settings.UseSwipesToMove = true;
                        settings.LessSound = true;
                        //settings.IsFirstRun = true;
                        GameLogger.AddLineToLog(string.Format("No settings found"));
                    }
                }
            }

            return settings;
        }

        public void SaveLocalSettings(GameSettings settings)
        {
            using (IsolatedStorageFile root = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var stream = new IsolatedStorageFileStream("dahlexsettings.xml", FileMode.Create, root))
                {
                    var serializer = new DataContractSerializer(typeof(GameSettings));
                    serializer.WriteObject(stream, settings);

                    GameLogger.AddLineToLog(string.Format("Settings saved"));
                }
            }
        }
    }
}