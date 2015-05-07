using System;
using System.Collections.Generic;
using Dahlex.Common;
using Dahlex.Logic.Contracts;
using Dahlex.Logic.Game;
using Dahlex.Logic.Logger;
using Dahlex.Logic.Settings;
using Dahlex.Logic.Utils;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Dahlex.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class GamePage : LayoutAwarePage, IDahlexView
    {
        private IGameController _dg;
        private GameSettings _settings;
        private GameMode _mode;
        private TimeSpan _elapsed = TimeSpan.Zero;
        private Popup _p;
        private DispatcherTimer _gameTimer;
        private readonly IDictionary<Guid, Storyboard> _boards = new Dictionary<Guid, Storyboard>();

        private enum FadeMode { None, FadeIn, FadeInDelayed, Hide };

        private FadeMode _prevFadeMode = FadeMode.None;

        public GamePage()
        {
            this.InitializeComponent();
        }

        private void GamePage_OnLoaded(object sender, RoutedEventArgs e)
        {
//            _settings = GetSettings();
            var boardDim = new IntSize(cnvMovement.ActualWidth, cnvMovement.ActualHeight);
            ISettingsManager sm = new SettingsManager(boardDim);
            _settings = sm.LoadLocalSettings();

            _dg = new GameController(this, _settings);
            DisableUserIdleDetection();


            if (SuspensionManager.SessionState.ContainsKey("Dahlex.Board"))
            // if (PhoneApplicationService.Current.State.ContainsKey("Dahlex.Board"))
            {
                var state = SuspensionManager.SessionState["Dahlex.Board"] as IGameState;
                //             var state = PhoneApplicationService.Current.State["Dahlex.Board"] as IGameState;
                if (state != null)
                {
                    // todo board and settings not loaded
                    _dg.SetState(state);
                    _elapsed = TimeSpan.FromSeconds(state.ElapsedTimeInSeconds);
                    ContinueGame((GameMode)state.Mode);
                    return;
                }
            }

            UpdateUI(GameStatus.BeforeStart, _dg.GetState(_elapsed));
        }

        private void GamePage_OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_p != null)
            {
                _p.IsOpen = false;
            }
            EnableUserIdleDetection();
        }

      /*  private GameSettings GetSettings()
        {
            ISettingsManager sm = new SettingsManager(1,2);
            var s = sm.LoadLocalSettings();
            return s;
        }*/

        private bool PerformRound(MoveDirection dir)
        {
            bool movedOk = false;

            if (_dg != null)
            {
                if (_dg.Status == GameStatus.LevelOngoing)
                {
                    _dg.MoveHeapsToTemp();
                    if (_dg.MoveProfessorToTemp(dir))
                    {
                        _dg.MoveRobotsToTemp();
                        _dg.CommitTemp();

                        movedOk = true;
                    }
                    else
                    {
                        AddLineToLog("P. not moved");
                    }
                }

                UpdateUI(_dg.Status, _dg.GetState(_elapsed));
            }
            return movedOk;
        }

        private void UpdateUI(GameStatus gameStatus, IGameState state)
        {
            if (gameStatus == GameStatus.BeforeStart)
            {
                btnBomb.IsEnabled = false;
                btnTeleport.IsEnabled = false;
            }
            else if (gameStatus == GameStatus.GameStarted)
            {
                AddLineToLog("Game started");
                btnBomb.IsEnabled = true;
                btnTeleport.IsEnabled = true;

                txtBoardMessage.Text = state.Message;
            }
            else if (gameStatus == GameStatus.LevelComplete)
            {
                AddLineToLog("Level won");
                btnBomb.IsEnabled = false;
                btnTeleport.IsEnabled = false;

                txtBoardMessage.Text = state.Message;
            }
            else if (gameStatus == GameStatus.LevelOngoing)
            {
                if (state.BombCount > 0)
                {
                    btnBomb.IsEnabled = true;
                }
                if (state.TeleportCount > 0)
                {
                    btnTeleport.IsEnabled = true;
                }

                txtBoardMessage.Text = state.Message;
            }
            else if (gameStatus == GameStatus.GameLost)
            {
                AddLineToLog("You lost");
                btnBomb.IsEnabled = false;
                btnTeleport.IsEnabled = false;
            }
            else if (gameStatus == GameStatus.GameWon)
            {
            }

            if (state.BombCount < 1)
            {
                btnBomb.IsEnabled = false;
            }

            if (state.TeleportCount < 1)
            {
                btnTeleport.IsEnabled = false;
            }

            BoardMessage(gameStatus);
        }

        private void BoardMessage(GameStatus gameStatus)
        {
            const double targetOpacity = 0.85;
            textMessage.FontWeight = FontWeights.Bold;
            if (gameStatus == GameStatus.BeforeStart)
            {
                if (SettingsManager.IsFirstRun())
                {
                    try
                    {
                        var major = Windows.ApplicationModel.Package.Current.Id.Version.Major;
                        var minor = Windows.ApplicationModel.Package.Current.Id.Version.Minor;

                        textMessage.Text = "Start First Game";
                        string message = string.Format("Welcome to Dahlex v{0}.{1}", major, minor);
                        popItUp(message,
                                "Now on Windows 8.x!" + Environment.NewLine +
                                "Enjoy!");
                    }
                    catch
                    { /* safety try */ }
                }
                else
                {
                    textMessage.Text = "Start New Game";
                }

                hyperMessage1.Content = string.Format("Random ({0} levels)", _settings.MaxNumberOfLevel);
                hyperMessage2.Content = "Tutorial (beta)";

                FadeIt(cnvMessage, FadeMode.FadeIn, 0, targetOpacity);
            }
            else if (gameStatus == GameStatus.GameStarted)
            {
                textMessage.Text = "";
                hyperMessage1.Content = "";
                hyperMessage2.Content = "";

                FadeIt(cnvMessage, FadeMode.Hide, targetOpacity, 0);
            }
            else if (gameStatus == GameStatus.LevelComplete)
            {
                if (_dg.AreThereNoMoreLevels) // otherwise this is discoverd too late
                {
                    gameStatus = GameStatus.GameWon;
                    _dg.AddHighScore(true);
                }
                else
                {
                    textMessage.Text = string.Format("Level {0} Won", _dg.CurrentLevel);
                    hyperMessage1.Content = "Next ->";
                    hyperMessage2.Content = "";

                    FadeIt(cnvMessage, FadeMode.FadeInDelayed, 0, targetOpacity);
                }
            }
            else if (gameStatus == GameStatus.LevelOngoing)
            {
                textMessage.Text = "";
                hyperMessage1.Content = "";
                hyperMessage2.Content = "";

                FadeIt(cnvMessage, FadeMode.Hide, targetOpacity, 0);
            }
            else if (gameStatus == GameStatus.GameLost)
            {
                textMessage.Text = "Game Over!";
                hyperMessage1.Content = "New Random Game";
                hyperMessage2.Content = "Start Tutorial";

                FadeIt(cnvMessage, FadeMode.FadeInDelayed, 0, targetOpacity);
            }

            if (gameStatus == GameStatus.GameWon)
            {
                textMessage.Text = "Game Complete!";
                if (_mode == GameMode.Random)
                {
                    hyperMessage1.Content = "";
                    hyperMessage2.Content = "Check High Scores";
                }
                else if (_mode == GameMode.Campaign)
                {
                    hyperMessage1.Content = "";
                    hyperMessage2.Content = "Start Random Game";
                }

                FadeIt(cnvMessage, FadeMode.FadeInDelayed, 0, targetOpacity);
            }
        }

        private void FadeIt(Canvas canvas, FadeMode fadeMode, double start, double end)
        {
            if (_prevFadeMode == fadeMode)
            {
                _prevFadeMode = fadeMode;
                return;
            }
            _prevFadeMode = fadeMode;

            storyMessages.Stop();
            storyMessages.Children.Clear();

            var opan = new DoubleAnimation();
            opan.From = start;
            opan.To = end;

            opan.AutoReverse = false;
            opan.RepeatBehavior = new RepeatBehavior(1);
            opan.FillBehavior = FillBehavior.HoldEnd;

            if (fadeMode == FadeMode.FadeInDelayed)
            {
                var easer = new PowerEase();
                easer.EasingMode = EasingMode.EaseIn;
                easer.Power = 10;

                opan.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 2000));
                opan.EasingFunction = easer;
            }
            else
            {
                opan.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 800));
            }

            Storyboard.SetTargetName(opan, cnvMessage.Name);
            Storyboard.SetTargetProperty(opan, "Opacity");

            storyMessages.Children.Add(opan);
            storyMessages.Begin();

            if (fadeMode == FadeMode.FadeInDelayed || fadeMode == FadeMode.FadeIn)
            {
                canvas.Visibility = Visibility.Visible;
                opan.Completed += opan_ClickableOnCompleted;
            }
            else
            {
                opan.Completed += opan_CollapseOnCompleted;
            }
        }

        private void opan_CollapseOnCompleted(object sender, object e)
        {
            cnvMessage.Visibility = Visibility.Collapsed;
        }

        private void opan_ClickableOnCompleted(object sender, object e)
        {
            hyperMessage1.Click += hyperMessage1_Click;
            hyperMessage2.Click += hyperMessage2_Click;
        }

        private void hyperMessage1_Click(object sender, RoutedEventArgs e)
        {
            var hl = (HyperlinkButton)sender;
            if (string.IsNullOrEmpty(hl.Content.ToString()))
            {
                // user managed to press the empty link
                return;
            }

            hyperMessage1.Click -= hyperMessage1_Click;
            hyperMessage2.Click -= hyperMessage2_Click;

            if (_dg.Status == GameStatus.BeforeStart)
            {
                btnStartGame_Click(sender, e);
            }
            else if (_dg.Status == GameStatus.GameStarted)
            {
            }
            else if (_dg.Status == GameStatus.LevelComplete)
            {
                if (_dg.AreThereNoMoreLevels) // otherwise this is discoverd too late
                {
                    btnStartGame_Click(sender, e);
                }
                else
                {
                    btnNextLevel_Click(sender, e);
                }
            }
            else if (_dg.Status == GameStatus.LevelOngoing)
            {
            }
            else if (_dg.Status == GameStatus.GameLost)
            {
                btnStartGame_Click(sender, e);
            }
            else if (_dg.Status == GameStatus.GameWon)
            {
            }
        }

        private void btnNextLevel_Click(object sender, RoutedEventArgs e)
        {
            if (_dg != null)
            {
                if (_dg.Status == GameStatus.LevelComplete)
                {
                    storyPanel.Resources.Clear();

                    _dg.StartNextLevel();

                    _gameTimer.Start();
                }

                UpdateUI(_dg.Status, _dg.GetState(_elapsed));
            }
        }

        private void btnStartGame_Click(object sender, RoutedEventArgs e)
        {
            StartGame(GameMode.Random);
        }

        private void btnStartCampaign_Click(object sender, RoutedEventArgs e)
        {
            StartGame(GameMode.Campaign);
        }

        private void StartGame(GameMode mode)
        {
            _mode = mode;
            if (_gameTimer != null)
            {
                _gameTimer.Stop();
            }
            else
            {
                _gameTimer = new DispatcherTimer();
                _gameTimer.Tick += timer_tick;
                _gameTimer.Interval = new TimeSpan(0, 0, 1);
            }

            storyPanel.Resources.Clear();

            _elapsed = TimeSpan.Zero;
            _gameTimer.Start();
            _dg.StartGame(mode);

            UpdateUI(GameStatus.GameStarted, _dg.GetState(_elapsed));
        }

        private void timer_tick(object state, object e)
        {
            if (_dg.Status == GameStatus.LevelOngoing)
            {
                _elapsed = _elapsed.Add(new TimeSpan(0, 0, 1));
                lblElapsed.Text = _elapsed.ToString();
            }
            else
            {
                _gameTimer.Stop();
            }
        }

        private void hyperMessage2_Click(object sender, RoutedEventArgs e)
        {
            var hl = (HyperlinkButton)sender;
            if (string.IsNullOrEmpty(hl.Content.ToString()))
            {
                // user managed to press the empty link
                return;
            }

            hyperMessage1.Click -= hyperMessage1_Click;
            hyperMessage2.Click -= hyperMessage2_Click;

            if (_dg.Status == GameStatus.BeforeStart)
            {
                btnStartCampaign_Click(sender, e);
            }
            else if (_dg.Status == GameStatus.GameStarted)
            {
            }
            else if (_dg.Status == GameStatus.LevelComplete)
            {
                if (_dg.AreThereNoMoreLevels) // otherwise this is discoverd too late
                {
                    if (_mode == GameMode.Random)
                    {
                        this.Frame.Navigate(typeof(ScoresPage));
                    }
                    else if (_mode == GameMode.Campaign)
                    {
                        btnStartGame_Click(sender, e);
                    }
                }
            }
            else if (_dg.Status == GameStatus.LevelOngoing)
            {
            }
            else if (_dg.Status == GameStatus.GameLost)
            {
                btnStartCampaign_Click(sender, e);
            }
            else if (_dg.Status == GameStatus.GameWon)
            {
            }
        }

        private void popItUp(string header, string text)
        {
            _p = new Popup();

            var border = new Border();
            border.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x12, 0x31, 0x08));
            border.BorderThickness = new Thickness(5.0);

            var panel1 = new StackPanel();
            panel1.Background = new SolidColorBrush(Colors.Orange);

            var buttonOk = new Button();
            buttonOk.Content = "Ok";
            buttonOk.Margin = new Thickness(4.0);
            buttonOk.FontSize = 24;
            buttonOk.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x12, 0x31, 0x08));
            buttonOk.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x12, 0x31, 0x08));
            buttonOk.Click += buttonOk_Click;

            var textblockH = new TextBlock();
            textblockH.Text = header;
            textblockH.FontSize = 24;
            textblockH.Margin = new Thickness(4.0);
            textblockH.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x12, 0x31, 0x08));

            var textblockT = new TextBlock();
            textblockT.Text = text;
            textblockT.FontSize = 17;
            textblockT.Margin = new Thickness(4.0);
            textblockT.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x12, 0x31, 0x08));

            panel1.Children.Add(textblockH);
            panel1.Children.Add(textblockT);
            panel1.Children.Add(buttonOk);
            border.Child = panel1;

            // Set the Child property of Popup to the border
            // which contains a stackpanel, textblock and button.
            _p.Child = border;

            // Set where the popup will show up on the screen.
            _p.VerticalOffset = 250;
            _p.HorizontalOffset = 200;

            // Open the popup.
            _p.IsOpen = true;
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            _p.IsOpen = false;
        }

        private static bool IsTap(Point p)
        {
            return Trig.IsTooSmallSwipe(p);
        }

        // ugly, just for now to make it work
        private bool IsWithinBounds(ManipulationCompletedRoutedEventArgs e)
        {
            int height = (_settings.SquareSize.Height + _settings.LineWidth.X) * _settings.BoardSize.Height + 3;
            int width = (_settings.SquareSize.Width + _settings.LineWidth.X) * _settings.BoardSize.Width + 3;

            var fwe = e.Container as FrameworkElement;
            if (fwe != null)
            {
                //string cName = e.ManipulationContainer.GetType().Name;
                //bool validControl = cName.Equals("Grid");
                //bool validControl = fwe.Name.Equals(DahlexContent.Name) || fwe.Name.Equals("imgProfessor");
                Point p = new Point(e.Position.X, e.Position.Y);
                //Debug.WriteLine(cName + " - " + fwe.Name + " " + p.ToString());

                if (fwe.Name.Equals(cnvMovement.Name))
                {
                    return 0 < p.Y
                        && p.Y <= height
                        && 0 < p.X
                        && p.X <= width;
                }

                if (fwe.Name.Equals("imgProfessor") || fwe.Name.StartsWith("imgHeap") || fwe.Name.StartsWith("imgRobot"))
                {
                    var trans = fwe.TransformToVisual(DahlexContent);
                    Point p2 = trans.TransformPoint(p);

                    return 0 < p2.Y
                        && p2.Y <= height
                        && 0 < p2.X
                        && p2.X <= width;
                }
            }
            return false;
        }

        private void RemoveLines()
        {
            lineDir.X1 = lineDir.X2;
            lineDir.Y1 = lineDir.Y2;

            guideNeSw.X1 = guideNeSw.X2;
            guideNeSw.Y1 = guideNeSw.Y2;

            guideNwSe.X1 = guideNwSe.X2;
            guideNwSe.Y1 = guideNwSe.Y2;
        }

        private void BtnBomb_OnClick(object sender, RoutedEventArgs e)
        {
            BlowBomb();
        }

        private void BlowBomb()
        {
            if (_dg != null)
            {
                if (_dg.Status == GameStatus.LevelOngoing)
                {
                    _dg.MoveHeapsToTemp();
                    if (_dg.BlowBomb())
                    {
                        try
                        {
                            DrawExplosionRadius(_dg.GetProfessorCoordinates());
                        }
                        catch// (Exception ex)
                        {
                            /* safety try, marketplace version crashes on samsung */
                            //MessageBox.Show(ex.Message);
                        }

                        PlaySound(Sound.Bomb);
                        if (_dg.MoveProfessorToTemp(MoveDirection.None))
                        {
                            _dg.MoveRobotsToTemp();
                            _dg.CommitTemp();
                        }
                    }
                    else
                    {
                        AddLineToLog("Cannot bomb");
                    }
                }

                UpdateUI(_dg.Status, _dg.GetState(_elapsed));
            }
        }

        private void DrawExplosionRadius(IntPoint pos)
        {
            int gridPenWidth = _settings.LineWidth.X;

            double oLeft1 = (pos.X + 1) * (_settings.SquareSize.Width + gridPenWidth) - (50);
            double oTop1 = (pos.Y + 1) * (_settings.SquareSize.Height + gridPenWidth) - (50);

            const int aniDur = 550;

            storyBomb.Stop();
            storyBomb.Children.Clear();

            ellipseBomb.Visibility = Visibility.Visible;

            // Width

            var daWidth = new DoubleAnimation();
            daWidth.From = 0;
            daWidth.To = 2;

            daWidth.AutoReverse = true;
            daWidth.RepeatBehavior = new RepeatBehavior(1);
            daWidth.FillBehavior = FillBehavior.HoldEnd;

            daWidth.Duration = new Duration(new TimeSpan(0, 0, 0, 0, aniDur));
            daWidth.Completed += daLeft_Completed;

            Storyboard.SetTargetName(daWidth, ellipseBomb.Name);
            Storyboard.SetTargetProperty(daWidth, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");

            storyBomb.Children.Add(daWidth);

            // Height

            var daHeight = new DoubleAnimation();
            daHeight.From = 0;
            daHeight.To = 2;

            daHeight.AutoReverse = true;
            daHeight.RepeatBehavior = new RepeatBehavior(1);
            daHeight.FillBehavior = FillBehavior.HoldEnd;

            daHeight.Duration = new Duration(new TimeSpan(0, 0, 0, 0, aniDur));

            Storyboard.SetTargetName(daHeight, ellipseBomb.Name);
            Storyboard.SetTargetProperty(daHeight, "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");

            storyBomb.Children.Add(daHeight);

            // Top

            ellipseBomb.SetValue(Canvas.TopProperty, oTop1);

            // Left

            ellipseBomb.SetValue(Canvas.LeftProperty, oLeft1);

            storyBomb.Begin();
        }

        private void daLeft_Completed(object sender, object e)
        {
            ellipseBomb.Visibility = Visibility.Collapsed;
        }

        public void Clear(bool all)
        {
            var drawer = new BoardDrawing(cnvMovement, storyFade, _settings);

            drawer.Clear(all);
        }

        public void PlaySound(Sound effect)
        {
            if (!_settings.LessSound)
            {
                switch (effect)
                {
                    case Sound.Bomb:
                        PlayBombSoundEffect();
                        break;
                    case Sound.Teleport:
                        PlayTeleportSoundEffect();
                        break;
                    case Sound.Crash:
                        PlayCrashSoundEffect();
                        break;
                }
            }
        }

        public void Animate(BoardPosition bp, IntPoint oldPosition, IntPoint newPosition, Guid roundId)
        {
            int xOffset = _settings.ImageOffset.X;
            int yOffset = _settings.ImageOffset.Y;
            int gridPenWidth = _settings.LineWidth.X;

            int oLeft = oldPosition.X * (_settings.SquareSize.Width + gridPenWidth) + xOffset;
            int oTop = oldPosition.Y * (_settings.SquareSize.Height + gridPenWidth) + yOffset;

            int nLeft = newPosition.X * (_settings.SquareSize.Width + gridPenWidth) + xOffset;
            int nTop = newPosition.Y * (_settings.SquareSize.Height + gridPenWidth) + yOffset;

            if (bp.Type == PieceType.Professor)
            {
                aniProfLeft.From = oLeft;
                aniProfLeft.To = nLeft;

                aniProfTop.From = oTop;
                aniProfTop.To = nTop;
                storyProf.Begin();

                //AddLineToLog(string.Format("AnimP {0}:{1} to {2}:{3}", oLeft, oTop, nLeft, nTop));
            }
            else if (bp.Type == PieceType.Robot)
            {
                Storyboard toDie;
                if (_boards.ContainsKey(roundId))
                {
                    toDie = _boards[roundId];
                }
                else
                {
                    toDie = new Storyboard();
                    storyPanel.Resources.Add(roundId.ToString(), toDie);
                    _boards.Clear();
                    _boards.Add(roundId, toDie);
                }
                //AddLineToLog(string.Format("AnimR {0} to {1}", oldPos.ToString(), newPos.ToString()));

                var aniL = new DoubleAnimation();
                aniL.AutoReverse = aniProfLeft.AutoReverse;
                aniL.Duration = new Duration(TimeSpan.FromMilliseconds(1400));
                aniL.RepeatBehavior = new RepeatBehavior(1);
                aniL.FillBehavior = FillBehavior.HoldEnd;
                Storyboard.SetTargetName(aniL, bp.ImageName);
                Storyboard.SetTargetProperty(aniL, "(Canvas.Left)");

                toDie.Children.Add(aniL);

                aniL.From = oLeft;
                aniL.To = nLeft;

                var aniT = new DoubleAnimation();
                aniT.AutoReverse = aniProfTop.AutoReverse;
                aniT.Duration = new Duration(TimeSpan.FromMilliseconds(1400));
                aniT.RepeatBehavior = new RepeatBehavior(1);
                aniT.FillBehavior = FillBehavior.HoldEnd;
                Storyboard.SetTargetName(aniT, bp.ImageName);
                Storyboard.SetTargetProperty(aniT, "(Canvas.Top)");

                toDie.Children.Add(aniT);

                aniT.From = oTop;
                aniT.To = nTop;

                aniT.Completed += ani_Completed;
                aniL.Completed += ani_Completed;
                AddLineToLog(string.Format("AK {0}", bp.ImageName));

                //toDie.Begin();
            }
        }

        private void ani_Completed(object sender, object e)
        {
            var ani = (DoubleAnimation)sender;
            ani.Completed -= ani_Completed;

            ani.From = ani.To;
            string name = Storyboard.GetTargetName(ani);

            RemoveLines();
            AddLineToLog(string.Format("DisA {0} {1} ", ani.From, name));
        }

        public void RemoveImage(string imageName)
        {
            var drawer = new BoardDrawing(cnvMovement, storyFade, _settings);
            drawer.RemoveImage(imageName);
        }

        public void StartTheRobots(Guid roundId)
        {
            Storyboard toDie;
            if (_boards.ContainsKey(roundId))
            {
                toDie = _boards[roundId];
                toDie.Begin();
            }
        }

        public void DrawLines()
        {
            var drawer = new BoardDrawing(cnvMovement, storyFade, _settings);
            drawer.DrawLines(cnvLines);
        }

        /// <summary>
        /// http://soundbible.com/1151-Grenade.html
        /// </summary>
        private void PlayBombSoundEffect()
        {
            mediaBomb.Play();
        }

        /// <summary>
        /// http://soundbible.com/830-Door-Unlock.html
        /// </summary>
        private void PlayCrashSoundEffect()
        {
            mediaCrash.Play();
        }

        /// <summary>
        /// http://soundbible.com/709-Bottle-Rocket.html
        /// </summary>
        private void PlayTeleportSoundEffect()
        {
            mediaTeleport.Play();
        }

        private void HyperMessage1_OnClick(object sender, RoutedEventArgs e)
        {
            // dummy handler
        }

        public void AddLineToLog(string log)
        {
            //log += string.Format(" RC:{0} PC:{1} Img:{2}", storyPanel.Children.Count, storyProf.Children.Count, cnvMovement.Children.Count);

            GameLogger.AddLineToLog(log);
        }

        public void DrawBoard(IBoard board, int xSize, int ySize)
        {
            var drawer = new BoardDrawing(cnvMovement, storyFade, _settings);
            drawer.DrawBoard(board, xSize, ySize);
        }

        public void ShowStatus(int level, int bombCount, int teleportCount, int robotCount, int moveCount, int maxLevel)
        {
            lblLevel.Text = string.Format("Level: {0}/{1} ", level, maxLevel);
            lblBombs.Text = string.Format("Dahlex: {0}  Moves: {1}", robotCount, moveCount);
            btnBomb.Content = string.Format("Bomb ({0})", bombCount);
            btnTeleport.Content = string.Format("Teleport ({0})", teleportCount);
        }

        private void BtnTeleport_OnClick(object sender, RoutedEventArgs e)
        {
            DoTeleport();
        }

        private void DoTeleport()
        {
            if (_dg != null)
            {
                if (_dg.Status == GameStatus.LevelOngoing)
                {
                    _dg.MoveHeapsToTemp();
                    if (_dg.DoTeleport())
                    {
                        PlaySound(Sound.Teleport);

                        _dg.MoveRobotsToTemp();
                        _dg.CommitTemp();
                    }
                    else
                    {
                        AddLineToLog("No more teleports");
                    }
                }

                UpdateUI(_dg.Status, _dg.GetState(_elapsed));
            }
        }

        private void Current_Deactivated(object sender, RoutedEventArgs e)
        {
            if (!SuspensionManager.SessionState.ContainsKey("Dahlex.Board"))
            {
                SuspensionManager.SessionState.Add("Dahlex.Board", _dg.GetState(_elapsed));
            }
            else
            {
                SuspensionManager.SessionState["Dahlex.Board"] = _dg.GetState(_elapsed);
            }
        }

        private void ContinueGame(GameMode mode)
        {
            _mode = mode;
            if (_gameTimer != null)
            {
                _gameTimer.Stop();
            }
            else
            {
                _gameTimer = new DispatcherTimer();
                _gameTimer.Tick += timer_tick;
                _gameTimer.Interval = new TimeSpan(0, 0, 1);
            }

            _gameTimer.Start();
            _dg.ContinueGame(mode);
            UpdateUI(_dg.Status, _dg.GetState(_elapsed));
        }

        private void EnableUserIdleDetection()
        {
            // PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
        }

        private void DisableUserIdleDetection()
        {
            // PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
        }

        private void DahlexContent_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            var fwe = e.Container as FrameworkElement;
            if (fwe != null)
            {
                bool validControl = fwe.Name.Equals(cnvMovement.Name) || fwe.Name.StartsWith("lineX_") || fwe.Name.StartsWith("lineY_") || fwe.Name.Equals("imgProfessor") || fwe.Name.StartsWith("imgHeap") || fwe.Name.StartsWith("imgRobot");

                if (_dg != null && validControl && _dg.Status == GameStatus.LevelOngoing)
                {
                    var trans = fwe.TransformToVisual(DahlexContent);
                    Point start = trans.TransformPoint(e.Position);

                    //  var start = new Point(e.ManipulationOrigin.X, e.ManipulationOrigin.Y);

                    lineDir.X1 = start.X - cnvLines.Margin.Left;
                    lineDir.Y1 = start.Y;
                    lineDir.X2 = start.X - cnvLines.Margin.Left;
                    lineDir.Y2 = start.Y;
                }
            }
        }

        private void DahlexContent_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            bool moved = false;
            if (_dg != null && _dg.Status == GameStatus.LevelOngoing)
            {
                Point p = new Point(e.Cumulative.Translation.X, e.Cumulative.Translation.Y);
                // very small swipe or tap is like clicking the professor in standard move mode
                if (IsTap(p) && IsWithinBounds(e))
                {
                    moved = PerformRound(MoveDirection.None);
                }
                else if (IsWithinBounds(e))
                {
                    var direction = Trig.GetSwipeDirection(p);
                    if (direction != MoveDirection.Ignore)
                    {
                        moved = PerformRound(direction);
                    }
                }
            }

            if (!moved)
            {
                RemoveLines();
            }
        }

        private void DahlexContent_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var fwe = e.Container as FrameworkElement;
            if (fwe != null)
            {
                //string cName = e.ManipulationContainer.GetType().Name;
                bool validControl = fwe.Name.Equals(cnvMovement.Name) || fwe.Name.Equals("imgProfessor") || fwe.Name.StartsWith("imgHeap") || fwe.Name.StartsWith("imgRobot");

                if (_dg != null && validControl && _dg.Status == GameStatus.LevelOngoing)
                {
                    //var trans = fwe.TransformToVisual(DahlexContent);
                    // Point totaldelta = trans.Transform(e.CumulativeManipulation.Translation);
                    Point p = new Point(lineDir.X1, lineDir.Y1);
                    Point totaldelta = new Point(e.Cumulative.Translation.X,
                                                 e.Cumulative.Translation.Y);

                    if (IsTap(totaldelta))
                    {
                        lineDir.Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    }
                    else
                    {
                        lineDir.Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));

                        var direction = Trig.GetSwipeDirection(totaldelta);
                        if (direction == MoveDirection.North)
                        {
                            totaldelta.X = 0;
                            totaldelta.Y = -150;
                        }
                        else if (direction == MoveDirection.NorthEast)
                        {
                            totaldelta.X = 110;
                            totaldelta.Y = -110;
                        }
                        else if (direction == MoveDirection.East)
                        {
                            totaldelta.X = 150;
                            totaldelta.Y = 0;
                        }
                        else if (direction == MoveDirection.SouthEast)
                        {
                            totaldelta.X = 110;
                            totaldelta.Y = 110;
                        }
                        else if (direction == MoveDirection.South)
                        {
                            totaldelta.X = 0;
                            totaldelta.Y = 150;
                        }
                        else if (direction == MoveDirection.SouthWest)
                        {
                            totaldelta.X = -110;
                            totaldelta.Y = 110;
                        }
                        else if (direction == MoveDirection.West)
                        {
                            totaldelta.X = -150;
                            totaldelta.Y = 0;
                        }
                        else if (direction == MoveDirection.NorthWest)
                        {
                            totaldelta.X = -110;
                            totaldelta.Y = -110;
                        }
                    }

                    Point dest = new Point(p.X + totaldelta.X, p.Y + totaldelta.Y);

                    lineDir.X2 = dest.X;
                    lineDir.Y2 = dest.Y;

                    guideNeSw.X1 = lineDir.X1 + 50;
                    guideNeSw.Y1 = lineDir.Y1 - 50;
                    guideNeSw.X2 = lineDir.X1 - 50;
                    guideNeSw.Y2 = lineDir.Y1 + 50;

                    guideNwSe.X1 = lineDir.X1 - 50;
                    guideNwSe.Y1 = lineDir.Y1 - 50;
                    guideNwSe.X2 = lineDir.X1 + 50;
                    guideNwSe.Y2 = lineDir.Y1 + 50;
                }
            }
        }

        private void DahlexContent_Tapped(object sender, TappedRoutedEventArgs e)
        {
            bool moved = false;
            if (_dg != null && _dg.Status == GameStatus.LevelOngoing)
            {
                Point p = e.GetPosition(null);
                // very small swipe or tap is like clicking the professor in standard move mode
                //if (IsTap(p))
                //{
                moved = PerformRound(MoveDirection.None);
                //}

                if (!moved)
                {
                    RemoveLines();
                }
            }
        }
    }
}
