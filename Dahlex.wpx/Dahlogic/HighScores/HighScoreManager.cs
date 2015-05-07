using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using Dahlex.Logic.Contracts;

namespace Dahlex.Logic.HighScores
{
    public class HighScoreManager
    {
        public HighScoreManager()
        {
            _scores = LoadLocalHighScores();
        }

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
            //   var moddedScores = new HighScore[20];
            using (IsolatedStorageFile root = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var stream = new IsolatedStorageFileStream("dahlexhighscores.xml", FileMode.OpenOrCreate, root))
                {
                    var serializer = new DataContractSerializer(typeof(List<HighScore>));
                    try
                    {
                        _scores = (List<HighScore>)serializer.ReadObject(stream);
                        _scores.Sort(new HighScoreComparer());
                        _scores = _scores.GetRange(0, Math.Min(_scores.Count, 20));

                        //_scores.CopyTo(0, moddedScores, 0, 20);
                    }
                    catch
                    {
                        _scores.Clear();
                    }
                }
            }

            return _scores;
        }

        public void SaveLocalHighScores()
        {
            using (IsolatedStorageFile root = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var stream = new IsolatedStorageFileStream("dahlexhighscores.xml", FileMode.Create, root))
                {
                    var serializer = new DataContractSerializer(typeof(List<HighScore>));
                    serializer.WriteObject(stream, _scores);
                }
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