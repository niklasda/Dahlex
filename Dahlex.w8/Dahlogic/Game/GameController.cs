using System;
using System.Text;
using Dahlex.Logic.Contracts;
using Dahlex.Logic.HighScores;
using Dahlex.Logic.Settings;
using Dahlex.Logic.Utils;

namespace Dahlex.Logic.Game
{
    public class GameController : IGameController
    {
        private readonly IDahlexView _boardView;
        private readonly GameSettings _settings;

        private GameStatus _gameStatus;
        private int _level;
        private int _bombCount;
        private int _teleportCount;
        private int _robotCount;
        private int _moveCount;
        private string _tail;

        private readonly IntSize _boardSize; // number of squares
        //private readonly bool _bombToHeap;
        private readonly IntSize _squareSize; // in pixels
        private int _maxLevel;
        private IBoard _board;
        private DateTime _startTime;
        private GameMode _gameMode;

        public GameController(IDahlexView view, GameSettings settings)
        {
            _boardView = view;
            _settings = settings;

            _boardSize = _settings.BoardSize;
            _squareSize = _settings.SquareSize;
            //   _bombToHeap = _settings.BombToHeap;
        }

        public GameStatus Status
        {
            get { return _gameStatus; }
        }

        public int CurrentLevel
        {
            get { return _level; }
        }

        public bool AreThereNoMoreLevels
        {
            get { return _level >= _maxLevel; }
        }

        public void StartGame(GameMode mode)
        {
            _startTime = DateTime.Now;
            _gameStatus = GameStatus.LevelOngoing;

            _level = 1;
            _moveCount = 0;
            _bombCount = 0;
            _teleportCount = 0;
            _gameMode = mode;

            if (_gameMode == GameMode.Random)
            {
                _maxLevel = _settings.MaxNumberOfLevel;
            }
            else if (_gameMode == GameMode.Campaign)
            {
                _maxLevel = Campaign1.Boards.Length - 1;
            }

            initNewLevel(_level);
        }

        /// <summary>
        /// Continue game from tombstone, called after setState
        /// </summary>
        public void ContinueGame(GameMode mode)
        {
            _gameMode = mode;
            _startTime = DateTime.Now; // todo put in state

            initOldLevel(_level);
        }

        /// <summary>
        /// Gather state to save to tombstone
        /// </summary>
        /// <param name="elapsed"></param>
        /// <returns></returns>
        public IGameState GetState(TimeSpan elapsed)
        {
            IGameState state = new GameState();
            state.Level = _level;
            state.MoveCount = _moveCount;
            state.BombCount = _bombCount;
            state.TeleportCount = _teleportCount;
            state.GameStatus = (int)_gameStatus;
            state.ElapsedTimeInSeconds = (int)elapsed.TotalSeconds;
            state.Mode = (int)_gameMode;
            state.Message = _tail;

            if (_board != null) // if called before game is started
            {
                BoardPosition[,] tmp = _board.TheBoard;
                var b = new StringBuilder(_boardSize.Width * _boardSize.Height);

                for (int x = 0; x < _boardSize.Width; x++) // TODO this is run every round, seems inefficient
                {
                    for (int y = 0; y < _boardSize.Height; y++)
                    {
                        if (tmp[x, y] == null)
                        {
                            b.Append(' ');
                        }
                        else
                        {
                            string firstChar = tmp[x, y].Type.ToString().Substring(0, 1);
                            b.Append(firstChar);
                        }
                    }
                }

                state.TheBoard = b.ToString();
            }

            return state;
        }

        /// <summary>
        /// Restore state from tombstone, called before continueGame
        /// </summary>
        /// <param name="state"></param>
        public void SetState(IGameState state)
        {
            _level = state.Level;
            _moveCount = state.MoveCount;
            _bombCount = state.BombCount;
            _teleportCount = state.TeleportCount;
            _gameStatus = (GameStatus)state.GameStatus;
            _gameMode = (GameMode)state.Mode;

            setBoard(state.TheBoard);
        }

        private void setBoard(string boardString)
        {
            var b = new BoardPosition[_boardSize.Width, _boardSize.Height];
            int i = 0;
            int heaps = 0;
            int robots = 0;
            for (int x = 0; x < _boardSize.Width; x++)
            {
                for (int y = 0; y < _boardSize.Height; y++)
                {
                    if (boardString[i] == 'P')
                    {
                        b[x, y] = new BoardPosition(PieceType.Professor, "imgProfessor");
                    }
                    else if (boardString[i] == 'R')
                    {
                        b[x, y] = new BoardPosition(PieceType.Robot, "imgRobot" + robots++);
                    }
                    else if (boardString[i] == 'H')
                    {
                        b[x, y] = new BoardPosition(PieceType.Heap, "imgHeap" + heaps++);
                    }
                    else
                    {
                        b[x, y] = null;
                    }
                    i++;
                }
            }

            _tail = boardString.Substring(i);

            _board = new BoardMatrix(_boardSize);
            _board.TheBoard = b;
        }

        public void StartNextLevel()
        {
            if (_level == _maxLevel)
            {
                // never happens
                //                _gameStatus = GameStatus.GameWon;
            }
            else
            {
                _gameStatus = GameStatus.LevelOngoing;
                _level++;

                initNewLevel(_level);
            }
        }

        /// <summary>
        /// Called from StartGame and startNextLevel
        /// </summary>
        /// <param name="thisLevel"></param>
        private void initNewLevel(int thisLevel)
        {
            _bombCount++;
            _teleportCount++;

            if (_gameMode == GameMode.Campaign)
            {
                setBoard(Campaign1.Boards[thisLevel]);

                _robotCount = _board.GetRobotCount();
            }
            else
            {
                _board = new BoardMatrix(_boardSize);
                _robotCount = thisLevel + 1;
                _tail = "";

                createProfessor();
                createRobots(_robotCount);
                createHeaps(thisLevel);
            }

            redraw(true);
        }

        /// <summary>
        /// Called from continueGame
        /// </summary>
        /// <param name="thisLevel"></param>
        private void initOldLevel(int thisLevel)
        {
            if (_board == null) // i.e. not restored from tombstone
            {
                _robotCount = thisLevel + 1;
                _board = new BoardMatrix(_boardSize);
                createProfessor();
                createRobots(_robotCount);
                createHeaps(thisLevel);
            }
            else
            {
                _robotCount = _board.GetRobotCount();
            }

            redraw(true);
        }

        private void createProfessor()
        {
            IntPoint profPos = getFreePosition();
            BoardPosition pPos = BoardPosition.CreateProfessorBoardPosition();
            _board.SetPosition(profPos.X, profPos.Y, pPos);
        }

        private void createRobots(int count)
        {
            removeOldPieces(PieceType.Heap);  // todo why, and why heaps
            for (int i = 0; i < count; i++)
            {
                IntPoint robotPos = getFreePosition();
                BoardPosition rPos = BoardPosition.CreateRobotBoardPosition(i);
                _board.SetPosition(robotPos.X, robotPos.Y, rPos);
            }
        }

        private void createHeaps(int count)
        {
            removeOldPieces(PieceType.Heap); // todo why
            for (int i = 0; i < count; i++)
            {
                IntPoint robotPos = getFreePosition();
                BoardPosition rPos = BoardPosition.CreateHeapBoardPosition(i);
                _board.SetPosition(robotPos.X, robotPos.Y, rPos);
            }
        }

        private void removeOldPieces(PieceType typeToRemove)
        {
            for (int x = 0; x < _board.GetPositionWidth(); x++)
            {
                for (int y = 0; y < _board.GetPositionHeight(); y++)
                {
                    if (_board.GetPosition(x, y) != null)
                    {
                        BoardPosition cp = _board.GetPosition(x, y);

                        if (cp.Type == typeToRemove)
                        {
                            _board.ResetPosition(x, y);
                        }
                    }
                }
            }
        }

        private IntPoint getFreePosition()
        {
            IntPoint p;
            do
            {
                p = Randomizer.GetRandomPosition(_boardSize.Width, _boardSize.Height);
            }
            while (_board.GetPosition(p.X, p.Y) != null);

            return new IntPoint(p.X, p.Y);
        }

        public void MoveHeapsToTemp()
        {
            for (int x = 0; x < _board.GetPositionWidth(); x++)
            {
                for (int y = 0; y < _board.GetPositionHeight(); y++)
                {
                    if (_board.GetPosition(x, y) != null)
                    {
                        BoardPosition cp = _board.GetPosition(x, y);

                        if (cp.Type == PieceType.Heap)
                        {
                            IntPoint point = new IntPoint(x, y);

                            moveCharacter(point, point, Guid.Empty); // no guid needed, doesn't move
                        }
                    }
                }
            }
        }

        private void moveCharacter(IntPoint oldPosition, IntPoint newPosition, Guid roundId)
        {
            BoardPosition oldBp = _board.GetPosition(oldPosition.X, oldPosition.Y);
            BoardPosition newBp = _board.GetTempPosition(newPosition.X, newPosition.Y);

            if (newBp == null || newBp.Type == PieceType.None)
            {
                _board.SetTempPosition(newPosition.X, newPosition.Y, oldBp);
                _boardView.Animate(oldBp, oldPosition, newPosition, roundId);
                _boardView.AddLineToLog("M. " + oldBp.Type + " to " + newPosition.ToString());
            }
            else if (oldBp.Type == PieceType.Robot && newBp.Type == PieceType.Robot)
            {
                _boardView.AddLineToLog("Robot-robot collision on " + newPosition.ToString());
                _boardView.Animate(oldBp, oldPosition, newPosition, roundId);

                _boardView.PlaySound(Sound.Crash);

                newBp.ConvertToHeap();
                _robotCount -= 2;
                if (_robotCount == 0)
                {
                    _gameStatus = GameStatus.LevelComplete;
                }
            }
            else if (oldBp.Type == PieceType.Robot && newBp.Type == PieceType.Heap)
            {
                _boardView.AddLineToLog("Robot-heap collision on " + newPosition.ToString());
                _boardView.Animate(oldBp, oldPosition, newPosition, roundId);

                _boardView.PlaySound(Sound.Crash);

                newBp.ConvertToHeap();
                _robotCount--;
                if (_robotCount == 0)
                {
                    _gameStatus = GameStatus.LevelComplete;
                }
            }
            else if (oldBp.Type == PieceType.Robot && newBp.Type == PieceType.Professor)
            {
                _boardView.AddLineToLog("Robot killed professor on " + newPosition.ToString());
                _boardView.Animate(oldBp, oldPosition, newPosition, roundId);

                _boardView.PlaySound(Sound.Crash);

                newBp.ConvertToHeap();
                _gameStatus = GameStatus.GameLost;
                AddHighScore(false);
            }
            else if (oldBp.Type == PieceType.Professor && newBp.Type == PieceType.Robot)
            {
                _boardView.AddLineToLog("Professor hit robot on " + newPosition.ToString());
                _boardView.Animate(oldBp, oldPosition, newPosition, roundId);

                _boardView.PlaySound(Sound.Crash);

                newBp.ConvertToHeap();
                _gameStatus = GameStatus.GameLost;
                AddHighScore(false);
            }
            else if (oldBp.Type == PieceType.Professor && newBp.Type == PieceType.Heap)
            {
                _boardView.AddLineToLog("Professor blocked by heap on " + newPosition.ToString());

                _board.SetTempPosition(oldPosition.X, oldPosition.Y, _board.GetPosition(oldPosition.X, oldPosition.Y));
            }
        }

        public IntPoint GetProfessorCoordinates()
        {
            IntPoint pos = getProfessor(false);
            return pos;
        }

        private IntPoint getProfessor(bool fromTemp)
        {
            if (fromTemp)
            {
                return _board.GetProfessorFromTemp();
            }
            else
            {
                return _board.GetProfessor();
            }
        }

        public bool MoveProfessorToTemp(MoveDirection dir)
        {
            IntPoint oldProfessorPosition = getProfessor(false);
            IntPoint newProfessorPosition = oldProfessorPosition;

            if (dir == MoveDirection.North)
            {
                if ((oldProfessorPosition.Y) > 0)
                {
                    newProfessorPosition.Y--;
                }
            }
            else if (dir == MoveDirection.East)
            {
                if ((oldProfessorPosition.X + 1) < _boardSize.Width)
                {
                    newProfessorPosition.X++;
                }
            }
            else if (dir == MoveDirection.South)
            {
                if ((oldProfessorPosition.Y + 1) < _boardSize.Height)
                {
                    newProfessorPosition.Y++;
                }
            }
            else if (dir == MoveDirection.West)
            {
                if ((oldProfessorPosition.X) > 0)
                {
                    newProfessorPosition.X--;
                }
            }
            else if (dir == MoveDirection.NorthEast)
            {
                if ((oldProfessorPosition.Y) > 0 && (oldProfessorPosition.X + 1) < _boardSize.Width)
                {
                    newProfessorPosition.Y--;
                    newProfessorPosition.X++;
                }
            }
            else if (dir == MoveDirection.SouthEast)
            {
                if ((oldProfessorPosition.Y + 1) < _boardSize.Height && (oldProfessorPosition.X + 1) < _boardSize.Width)
                {
                    newProfessorPosition.Y++;
                    newProfessorPosition.X++;
                }
            }
            else if (dir == MoveDirection.SouthWest)
            {
                if ((oldProfessorPosition.Y + 1) < _boardSize.Height && (oldProfessorPosition.X) > 0)
                {
                    newProfessorPosition.Y++;
                    newProfessorPosition.X--;
                }
            }
            else if (dir == MoveDirection.NorthWest)
            {
                if ((oldProfessorPosition.Y) > 0 && (oldProfessorPosition.X) > 0)
                {
                    newProfessorPosition.Y--;
                    newProfessorPosition.X--;
                }
            }
            else if (dir == MoveDirection.None)
            {
            }
            else
            {
                throw new Exception("No direction specified in move");
            }

            if (!oldProfessorPosition.Equals(newProfessorPosition) || (dir == MoveDirection.None))
            {
                moveCharacter(oldProfessorPosition, newProfessorPosition, Guid.Empty); // no guid needed, prof has own storyboard
                _moveCount++;
                return true;
            }

            return false;
        }

        public void MoveRobotsToTemp()
        {
            IntPoint prof = getProfessor(true);
            var guid = Guid.NewGuid();

            for (int x = 0; x < _board.GetPositionWidth(); x++)
            {
                for (int y = 0; y < _board.GetPositionHeight(); y++)
                {
                    if (_board.GetPosition(x, y) != null)
                    {
                        BoardPosition cp = _board.GetPosition(x, y);
                        var current = new IntPoint(x, y);

                        if (cp.Type == PieceType.Robot)
                        {
                            var diff = new IntPoint(Math.Sign(prof.X - current.X), Math.Sign(prof.Y - current.Y));
                            var newPoint = new IntPoint(current.X + diff.X, current.Y + diff.Y);

                            moveCharacter(current, newPoint, guid);
                        }
                    }
                }
            }
            _boardView.StartTheRobots(guid);
        }

        public void CommitTemp()
        {
            for (int x = 0; x < _board.GetPositionWidth(); x++)
            {
                for (int y = 0; y < _board.GetPositionHeight(); y++)
                {
                    var tempPosition = _board.GetTempPosition(x, y);

                    _board.SetPosition(x, y, tempPosition);
                    _board.ResetTempPosition(x, y);
                }
            }

            redraw(false);
        }

        public void AddHighScore(bool maxLevel)
        {
            //            var sm = new SettingsManager();
            var hsm = new HighScoreManager();

            string name = _settings.PlayerName;

            if (maxLevel)
            {
                _level = SettingsManager.MaxLevelIndicator;
            }

            hsm.AddHighScore(_gameMode, name, _level, _bombCount, _teleportCount, _moveCount, _startTime, _boardSize);
            hsm.SaveLocalHighScores();
        }

        private void redraw(bool clear)
        {
            if (clear)
            {
                _boardView.Clear(true);
            }

            _boardView.DrawBoard(_board, _squareSize.Width, _squareSize.Height);

            _boardView.ShowStatus(_level, _bombCount, _teleportCount, _robotCount, _moveCount, _maxLevel);
        }

        public bool BlowBomb()
        {
            int robotCountBefore = _robotCount;
            Guid roundId = Guid.NewGuid();

            if (_bombCount > 0)
            {
                IntPoint prof = getProfessor(false);

                for (int x = Math.Max(prof.X - 1, 0); x <= Math.Min(prof.X + 1, _boardSize.Width - 1); x++)
                {
                    for (int y = Math.Max(prof.Y - 1, 0); y <= Math.Min(prof.Y + 1, _boardSize.Height - 1); y++)
                    {
                        if (_board.GetPosition(x, y) != null)
                        {
                            BoardPosition bp = _board.GetPosition(x, y);

                            if (bp.Type == PieceType.Robot)
                            {
                                _boardView.AddLineToLog(string.Format("Bombing robot {0}", (new IntPoint(x, y)).ToString()));
                                _boardView.Animate(bp, new IntPoint(x, y), new IntPoint(x, y), roundId);

                                //_boardView.RemoveRobotAnimation(bp);

                                _board.SetTempPosition(x, y, bp);
                                bp.ConvertToNone();
                                _boardView.RemoveImage(bp.ImageName);
                                //  }

                                _robotCount--;
                                if (_robotCount == 0)
                                {
                                    _gameStatus = GameStatus.LevelComplete;
                                }
                            }
                        }
                    }
                }
            }

            if (robotCountBefore != _robotCount)
            {
                _bombCount--;
                return true;
            }

            return false;
        }

        public bool DoTeleport()
        {
            if (_teleportCount > 0)
            {
                IntPoint oldProfPos = getProfessor(false);
                IntPoint newProfPos = getFreePosition();

                _boardView.AddLineToLog(string.Format("T. from {0} to {1}", oldProfPos.ToString(), newProfPos.ToString()));

                moveCharacter(oldProfPos, newProfPos, Guid.Empty); // no guid for prof.
                _moveCount++;
                _teleportCount--;
                return true;
            }
            return false;
        }
    }
}