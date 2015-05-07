using System.Runtime.Serialization;
using Dahlex.Logic.Contracts;

namespace Dahlex.Logic.Game
{
    [DataContract]
    public class GameState : IGameState
    {
        private int _level;
        private int _moveCount;
        private int _bombCount;
        private int _teleportCount;
        private int _gameStatus;
        private int _elapsedSeconds;
        private string _theBoard;
        private int _mode;
        private string _message;

        [DataMember]
        public int Level
        {
            get { return _level; }
            set { _level = value; }
        }

        [DataMember]
        public int MoveCount
        {
            get { return _moveCount; }
            set { _moveCount = value; }
        }

        [DataMember]
        public int BombCount
        {
            get { return _bombCount; }
            set { _bombCount = value; }
        }

        [DataMember]
        public int TeleportCount
        {
            get { return _teleportCount; }
            set { _teleportCount = value; }
        }

        [DataMember]
        public int GameStatus
        {
            get { return _gameStatus; }
            set { _gameStatus = value; }
        }

        [DataMember]
        public int ElapsedTimeInSeconds
        {
            get { return _elapsedSeconds; }
            set { _elapsedSeconds = value; }
        }

        [DataMember]
        public string TheBoard
        {
            get { return _theBoard; }
            set { _theBoard = value; }
        }

        [DataMember]
        public int Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        [IgnoreDataMember]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
    }
}