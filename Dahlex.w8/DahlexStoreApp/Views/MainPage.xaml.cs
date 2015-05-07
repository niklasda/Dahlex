using System;
using System.Collections.Generic;
using Dahlex.Common;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Dahlex.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : LayoutAwarePage
    {
        public MainPage()
        {
            this.InitializeComponent();
            SettingsPane.GetForCurrentView().CommandsRequested += BlankPage_CommandsRequested;
        }

        private Popup _settingsPopup;

        private void BlankPage_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {

            var cmdAbout = new SettingsCommand("AboutCommand",
                 "About", (x) =>
                 {
                     _settingsPopup = new Popup();
                     _settingsPopup.Closed += OnPopupClosed;
                     Window.Current.Activated += OnWindowActivated;
                     _settingsPopup.IsLightDismissEnabled = true;
                     _settingsPopup.Width = 346;
                     _settingsPopup.Height = Window.Current.Bounds.Height;

                     var mypane = new AboutFlyout();
                     mypane.Width = 346;
                     mypane.Height = Window.Current.Bounds.Height;

                     _settingsPopup.Child = mypane;
                     _settingsPopup.SetValue(Canvas.LeftProperty, Window.Current.Bounds.Width - 346);
                     _settingsPopup.SetValue(Canvas.TopProperty, 0);
                     _settingsPopup.IsOpen = true;
                 });

            var cmdSettings = new SettingsCommand("SettingsCommand",
                 "Settings", (x) =>
                 {
                     _settingsPopup = new Popup();
                     _settingsPopup.Closed += OnPopupClosed;
                     Window.Current.Activated += OnWindowActivated;
                     _settingsPopup.IsLightDismissEnabled = true;
                     _settingsPopup.Width = 346;
                     _settingsPopup.Height =  Window.Current.Bounds.Height;

                     var mypane = new SettingsFlyout();
                     mypane.Width = 346;
                     mypane.Height = Window.Current.Bounds.Height;

                     _settingsPopup.Child = mypane;
                     _settingsPopup.SetValue(Canvas.LeftProperty, Window.Current.Bounds.Width - 346);
                     _settingsPopup.SetValue(Canvas.TopProperty, 0);
                     _settingsPopup.IsOpen = true;
                 });

            args.Request.ApplicationCommands.Clear();
            args.Request.ApplicationCommands.Add(cmdSettings);
            args.Request.ApplicationCommands.Add(cmdAbout);
        }

        private void OnWindowActivated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                _settingsPopup.IsOpen = false;
            }
        }

        private void OnPopupClosed(object sender, object e)
        {
            Window.Current.Activated -= OnWindowActivated;
        }

        //      private void orient()
        //    {
        //     
        //    buttonPlay.Margin = new Thickness(340, 350, 0, 0);// left top right bottom 163,570,0,0
        //}

        private void ButtonScores_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ScoresPage));
        }

        private void ButtonHow_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HowPage));
        }

        private void ButtonPlay_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(GamePage));
        }
    }
}
