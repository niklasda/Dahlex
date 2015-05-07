using System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Dahlex.Views
{
    public sealed partial class AboutFlyout : UserControl
    {
        public AboutFlyout()
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

        private void AboutFlyout_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var major = Windows.ApplicationModel.Package.Current.Id.Version.Major;
                var minor = Windows.ApplicationModel.Package.Current.Id.Version.Minor;

                lblAbout.Text = "Version: " + major + "." + minor + Environment.NewLine +
                                "Programming by Niklas Dahlman" + Environment.NewLine +
                                "Graphics by Peter Kleine." + Environment.NewLine +
                                "Swiping by Erik Chrissopoulos." + Environment.NewLine +
                                "" + Environment.NewLine +
                                "Send questions and level designs to: games@dahlman.biz" + Environment.NewLine +
                                "" + Environment.NewLine +
                                "Bomb Sound 1151 from Soundbible.com." + Environment.NewLine +
                                "Teleport Sound 709 from Soundbible.com." + Environment.NewLine +
                                "Collision Sound 930 from Soundbible.com." + Environment.NewLine +
                                "" + Environment.NewLine +
                    // "OS: " + Environment.OSVersion + Environment.NewLine +
                    // "CLR: " + Environment.Version + Environment.NewLine +
                    // "Device: " + string.Format("{0:#,#,#} Mb", deviceTotalMemory / 1024 / 1024) +
                    //  " - App: " +
                    //  string.Format("{0:#,#,#} Mb", appCurrentMemoryUsage / 1024 / 1024) +
                    //  " - Peak: " + string.Format("{0:#,#,#} Mb", appPeakMemoryUsage / 1024 / 1024) +
                                Environment.NewLine;
            }
            catch (Exception ex)
            {
                /* safety try */
                lblAbout.Text = ex.Message;
            }
        }
    }
}
