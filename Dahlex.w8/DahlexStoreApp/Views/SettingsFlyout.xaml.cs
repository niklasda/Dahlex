using Dahlex.Logic.Contracts;
using Dahlex.Logic.Settings;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Dahlex.Views
{
    public sealed partial class SettingsFlyout : UserControl
    {
        public SettingsFlyout()
        {
            this.InitializeComponent();
        }

        private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.Parent is Popup)
            {
                ((Popup)this.Parent).IsOpen = false;
            }
            SettingsPane.Show();
        }

        private GameSettings _settings;// = new GameSettings();

        private void SettingsFlyout_OnLoaded(object sender, RoutedEventArgs e)
        {
            ISettingsManager man = new SettingsManager(null);
            _settings = man.LoadLocalSettings();

            tsLessSound.IsOn = _settings.LessSound;

            if (!string.IsNullOrEmpty(_settings.PlayerName))
            {
                txtPlayerName.Text = _settings.PlayerName;
            }
        }

        private void SettingsFlyout_OnUnloaded(object sender, RoutedEventArgs e)
        {
            _settings.LessSound = tsLessSound.IsOn;
            _settings.PlayerName = txtPlayerName.Text;

            ISettingsManager man = new SettingsManager(null);
            man.SaveLocalSettings(_settings);
        }
    }
}
