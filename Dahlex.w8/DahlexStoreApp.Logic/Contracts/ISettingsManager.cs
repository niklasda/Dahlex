using Dahlex.Logic.Settings;

namespace Dahlex.Logic.Contracts
{
    public interface ISettingsManager
    {
        GameSettings LoadLocalSettings();

        void SaveLocalSettings(GameSettings settings);
    }
}