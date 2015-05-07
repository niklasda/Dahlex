using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace Dahlex.Views
{
    public partial class SettingsPanoPage : PhoneApplicationPage
    {
        public SettingsPanoPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
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