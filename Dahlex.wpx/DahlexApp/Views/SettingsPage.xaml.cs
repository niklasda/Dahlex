using System.Windows;
using System.Windows.Controls;
using Dahlex.Logic.Contracts;
using Dahlex.Logic.Settings;
using Microsoft.Phone.Controls;

namespace Dahlex.Views
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            LoadSettings();
        }

        private GameSettings _settings = new GameSettings();
        private const string _soundsTextOff = "Sounds";
        private const string _soundsTextOn = "Mute";

        private void LoadSettings()
        {
            ISettingsManager man = new SettingsManager();
            _settings = man.LoadLocalSettings();

            tsLessSound.IsChecked = _settings.LessSound;

            if (tsLessSound.IsChecked.Value)
            {
                tsLessSound.Content = _soundsTextOn;
            }
            else
            {
                tsLessSound.Content = _soundsTextOff;
            }

            if (!string.IsNullOrEmpty(_settings.PlayerName))
            {
                txtPlayerName.Text = _settings.PlayerName;
            }
        }

        private void Save()
        {
            if (tsLessSound.IsChecked.HasValue)
            {
                _settings.LessSound = tsLessSound.IsChecked.Value;
            }

            _settings.UseSwipesToMove = true;

            _settings.BombToHeap = false;

            _settings.PlayerName = txtPlayerName.Text;

            ISettingsManager man = new SettingsManager();
            man.SaveLocalSettings(_settings);
        }

        private void tsLessSound_Checked(object sender, RoutedEventArgs e)
        {
            tsLessSound.Content = _soundsTextOn;
            Save();
        }

        private void tsLessSound_Unchecked(object sender, RoutedEventArgs e)
        {
            tsLessSound.Content = _soundsTextOff;
            Save();
        }

        private void txtPlayerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Save();
        }
    }
}