using System;
using System.Reflection;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Microsoft.Phone.Tasks;

namespace Dahlex.Views
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void AboutPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                long deviceTotalMemory = (long)DeviceExtendedProperties.GetValue("DeviceTotalMemory");
                long appCurrentMemoryUsage = (long)DeviceExtendedProperties.GetValue("ApplicationCurrentMemoryUsage");
                long appPeakMemoryUsage = (long)DeviceExtendedProperties.GetValue("ApplicationPeakMemoryUsage");

                string ver = Assembly.GetExecutingAssembly().GetName().Version.ToString(2);
                // the normal way above is not good for wp7 yet

                lblAbout.Text = "Version: " + ver + Environment.NewLine +
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
                                "OS: " + Environment.OSVersion + Environment.NewLine +
                                "CLR: " + Environment.Version + Environment.NewLine +
                                "Device: " + string.Format("{0:#,#,#} Mb", deviceTotalMemory / 1024 / 1024) +
                                " - App: " +
                                string.Format("{0:#,#,#} Mb", appCurrentMemoryUsage / 1024 / 1024) +
                                " - Peak: " + string.Format("{0:#,#,#} Mb", appPeakMemoryUsage / 1024 / 1024) +
                                Environment.NewLine;
            }
            catch (Exception ex)
            {
                /* safety try */
                lblAbout.Text = ex.Message;
            }
        }

        private void btnHomepage_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();

            marketplaceReviewTask.Show();

        }
    }
}