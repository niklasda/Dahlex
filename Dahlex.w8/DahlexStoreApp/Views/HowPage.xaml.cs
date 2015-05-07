using System;
using System.Collections.Generic;
using Dahlex.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Dahlex.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class HowPage : LayoutAwarePage
    {
        public HowPage()
        {
            this.InitializeComponent();
        }

        

        private void HowPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            lblInstructions.Text =
               "Dahlex is a turn-based platform game." + Environment.NewLine + Environment.NewLine +
               "Press Play button on the robot on the start page to start." + Environment.NewLine + Environment.NewLine +
               "The dahlex (i.e. robots) move straight towards the professor (you) striving to annihilate him." + Environment.NewLine + Environment.NewLine +
               "Move the professor so the dahlex hit the mines or collide with each other." + Environment.NewLine + Environment.NewLine +
               "Swipe to move the professor in the desired direction." + Environment.NewLine + Environment.NewLine +
               "Tap the professor and he will stand still while the dahlex move towards it." + Environment.NewLine + Environment.NewLine +
               "Tap 'Bomb' to blow a bomb, killing all adjecent dahlex." + Environment.NewLine + Environment.NewLine +
               "Tap 'Teleport' to teleport the professor, then the dahlex will move towards him, so he might be killed!" + Environment.NewLine + Environment.NewLine +
               "If 'Bomb' is pressed the professor will not move, but the dahlex will." + Environment.NewLine;
        }
    }
}
