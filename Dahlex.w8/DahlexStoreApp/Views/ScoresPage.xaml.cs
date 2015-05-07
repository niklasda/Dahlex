using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Dahlex.Common;
using Dahlex.Logic.Contracts;
using Dahlex.Logic.HighScores;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Dahlex.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ScoresPage : LayoutAwarePage
    {
        public ScoresPage()
        {
            this.InitializeComponent();
        }

        private void DataBindLocalHighScores()
        {
            var hsm = new HighScoreManager();
            List<HighScore> scores = hsm.LoadLocalHighScores();

            if (scores.Count == 0)
            {
                scores = new List<HighScore>(1);
                scores.Add(new HighScore("No scores yet...", 0, 0, 0, 0, DateTime.Now, new IntSize(1, 1)));
            }

            lstLocalScores.ItemsSource = scores;
            lstLocalScores.DisplayMemberPath = "Content";
        }

        //private void DataBindRemoteHighScores()
        //{
        //    var hsm = new LeaderboardService();
        //    var scoresTask = hsm.GetLeaderboard();
        //    var scores = scoresTask.Result;

            
        //}

        private void ScoresPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            DataBindLocalHighScores();
            //DataBindRemoteHighScores();
        }
    }
}
