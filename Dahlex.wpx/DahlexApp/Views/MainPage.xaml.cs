using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace Dahlex.Views
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            orient();
        }

        private void orient()
        {
            if (this.Orientation == PageOrientation.Portrait || this.Orientation == PageOrientation.PortraitUp || this.Orientation == PageOrientation.PortraitDown)
            {
                buttonPlay.Margin = new Thickness(176, 583, 0, 0); // left top right bottom 163,570,0,0
            }
            else if (this.Orientation == PageOrientation.Landscape || this.Orientation == PageOrientation.LandscapeRight || this.Orientation == PageOrientation.LandscapeLeft)
            {
                buttonPlay.Margin = new Thickness(340, 350, 0, 0);
            }
        }

        private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            orient();
        }

        private void buttonPlay_Click(object sender, RoutedEventArgs e)
        {
            var theUri = new Uri("/Views/GamePage.xaml", UriKind.Relative);
            NavigationService.Navigate(theUri);
        }

        private void buttonSettings_Click(object sender, RoutedEventArgs e)
        {
            var theUri = new Uri("/Views/SettingsPage.xaml", UriKind.Relative);
            NavigationService.Navigate(theUri);
        }

        private void buttonScores_Click(object sender, RoutedEventArgs e)
        {
            var theUri = new Uri("/Views/ScoresPage.xaml", UriKind.Relative);
            NavigationService.Navigate(theUri);
        }

        private void buttonAbout_Click(object sender, RoutedEventArgs e)
        {
            var theUri = new Uri("/Views/AboutPage.xaml", UriKind.Relative);
            NavigationService.Navigate(theUri);
        }

        private void buttonHow_Click(object sender, RoutedEventArgs e)
        {
            var theUri = new Uri("/Views/HowPanoPage.xaml", UriKind.Relative);
            NavigationService.Navigate(theUri);
        }
    }
}