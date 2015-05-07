using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Dahlex.Logic.Contracts;
using Windows.Storage;

namespace Dahlex.Logic.HighScores
{
    public class HighScoreManager
    {
        public HighScoreManager()
        {
            _scores = LoadLocalHighScores();
        }

        //  private int _max;
        private List<HighScore> _scores = new List<HighScore>();

        public void AddHighScore(GameMode mode, string name, int level, int bombsLeft, int teleportsLeft, int moves, DateTime startTime, IntSize boardSize)
        {
            if (mode == GameMode.Random)
            {
                var hs = new HighScore(name, level, bombsLeft, teleportsLeft, moves, startTime, boardSize);
                _scores.Add(hs);
            }
        }

        public List<HighScore> LoadLocalHighScores()
        {
            try
            {
                var settings = ApplicationData.Current.LocalSettings;
                string highScores = settings.Values["HighScores"].ToString();
                var bytes = Encoding.Unicode.GetBytes(highScores.ToCharArray());

                var serializer = new DataContractSerializer(typeof(List<HighScore>));

                _scores = (List<HighScore>)serializer.ReadObject(new MemoryStream(bytes));

                if (_scores.Count == 0)
                {
                    _scores.Add(new HighScore("Niklas (Beat me, I wanna be last)", 1, 1, 1, 1, DateTime.Now.AddMinutes(-3), new IntSize(12, 12)));
                }

                _scores.Sort(new HighScoreComparer());
                _scores = _scores.GetRange(0, Math.Min(_scores.Count, 20));
            }
            catch
            {
                _scores.Clear();
            }

            return _scores;
        }

        public void SaveLocalHighScores()
        {
            var serializer = new DataContractSerializer(typeof(List<HighScore>));
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                serializer.WriteObject(writer, _scores);
                writer.Flush();

                var settings = ApplicationData.Current.LocalSettings;
                settings.Values["HighScores"] = sb.ToString();
            }
        }

        internal class HighScoreComparer : IComparer<HighScore>
        {
            public int Compare(HighScore x, HighScore y)
            {
                if (x == null && y == null)
                {
                    return 0;
                }
                if (x == null && y != null)
                {
                    return 1;
                }
                if (x != null && y == null)
                {
                    return -1;
                }
                int cmp = y.Score.CompareTo(x.Score);
                if (cmp == 0)
                {
                    return x.GameDuration.CompareTo(y.GameDuration);
                }
                else
                {
                    return cmp;
                }
            }
        }
    }
}