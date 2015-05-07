using System;
using System.Collections.Generic;
using System.Windows;
using Dahlex.Logic.Contracts;
using Dahlex.Logic.HighScores;
using Microsoft.Phone.Controls;

namespace Dahlex.Views
{
    public partial class ScoresPage : PhoneApplicationPage
    {
        public ScoresPage()
        {
            InitializeComponent();
        }

        private void ScoresPage_Loaded(object sender, RoutedEventArgs e)
        {
            dataBindHighScores();
        }

        private void dataBindHighScores()
        {
            //            var sm = new SettingsManager();
            var hsm = new HighScoreManager();
            List<HighScore> scores = hsm.LoadLocalHighScores();

            if (scores.Count == 0)
            {
                scores = new List<HighScore>(1);
                scores.Add(new HighScore("No scores yet...", 0, 0, 0, 0, DateTime.Now, new IntSize(1, 1)));
            }

            lstScores.ItemsSource = scores;
            lstScores.DisplayMemberPath = "Content";
        }
    }
}