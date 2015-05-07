using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Dahlex.Logic.Contracts;
using Dahlex.Logic.Game;
using Dahlex.Logic.Logger;
using Dahlex.Logic.Settings;
using Dahlex.Logic.Utils;
using Microsoft.Devices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Color = System.Windows.Media.Color;
using Environment = System.Environment;
using Point = System.Windows.Point;

namespace Dahlex.Views
{
    public partial class GamePage : PhoneApplicationPage, IDahlexView
    {
        private IGameController _dg;
        private readonly GameSettings _settings;
        private GameMode _mode;

        public GamePage()
        {
            InitializeComponent();

            //cnvMessage.Visibility = Visibility.Collapsed;

            _settings = GetSettings();
            PhoneApplicationService.Current.Deactivated += Current_Deactivated;

            this.ManipulationStarted += PhoneApplicationPage_ManipulationStarted;
            this.ManipulationDelta += PhoneApplicationPage_ManipulationDelta;
            this.ManipulationCompleted += PhoneApplicationPage_ManipulationCompleted;
        }

        private void Current_Deactivated(object sender, DeactivatedEventArgs e)
        {
            if (!PhoneApplicationService.Current.State.ContainsKey("Dahlex.Board"))
            {
                PhoneApplicationService.Current.State.Add("Dahlex.Board", _dg.GetState(_elapsed));
            }
            else
            {
                PhoneApplicationService.Current.State["Dahlex.Board"] = _dg.GetState(_elapsed);
            }
        }

        private GameSettings GetSettings()
        {
            ISettingsManager sm = new SettingsManager();
            var s = sm.LoadLocalSettings();
            return s;
        }

        private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_p != null)
            {
                _p.IsOpen = false;
            }
            EnableUserIdleDetection();
        }

        private void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            _dg = new GameController(this, _settings);
            DisableUserIdleDetection();

            if (PhoneApplicationService.Current.State.ContainsKey("Dahlex.Board"))
            {
                var state = PhoneApplicationService.Current.State["Dahlex.Board"] as IGameState;
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

        // http://msdn.microsoft.com/en-us/library/ff941090(v=VS.92).aspx
        // Screen saver goes on, but app keeps running

        private void DisableUserIdleDetection()
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
        }

        private void EnableUserIdleDetection()
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
        }

        /// <summary>
        /// used when swiping
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PhoneApplicationPage_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            var fwe = e.ManipulationContainer as FrameworkElement;
            if (fwe != null)
            {
                bool validControl = fwe.Name.Equals(DahlexContent.Name) || fwe.Name.Equals("imgProfessor") || fwe.Name.StartsWith("imgHeap") || fwe.Name.StartsWith("imgRobot");

                // string cName = e.ManipulationContainer.GetType().Name;
                // bool validControl = cName.Equals(DahlexContent.Name) || cName.Equals("imgProfessor");
                // bool validControl = e.ManipulationContainer.GetType().Name.Equals("Grid");

                if (_dg != null && validControl && _dg.Status == GameStatus.LevelOngoing)
                {
                    var trans = fwe.TransformToVisual(DahlexContent);
                    Point start = trans.Transform(e.ManipulationOrigin);

                    //  var start = new Point(e.ManipulationOrigin.X, e.ManipulationOrigin.Y);

                    lineDir.X1 = start.X;
                    lineDir.Y1 = start.Y;
                    lineDir.X2 = start.X;
                    lineDir.Y2 = start.Y;
                }
            }
        }

        /// <summary>
        /// using when swiping
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PhoneApplicationPage_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            var fwe = e.ManipulationContainer as FrameworkElement;
            if (fwe != null)
            {
                //string cName = e.ManipulationContainer.GetType().Name;
                bool validControl = fwe.Name.Equals(DahlexContent.Name) || fwe.Name.Equals("imgProfessor") || fwe.Name.StartsWith("imgHeap") || fwe.Name.StartsWith("imgRobot");

                if (_dg != null && validControl && _dg.Status == GameStatus.LevelOngoing)
                {
                    //var trans = fwe.TransformToVisual(DahlexContent);
                    // Point totaldelta = trans.Transform(e.CumulativeManipulation.Translation);
                    Point p = new Point(lineDir.X1, lineDir.Y1);
                    Point totaldelta = new Point(e.CumulativeManipulation.Translation.X,
                                                 e.CumulativeManipulation.Translation.Y);

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

        /// <summary>
        /// used when swiping
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PhoneApplicationPage_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            bool moved = false;
            if (_dg != null && _dg.Status == GameStatus.LevelOngoing)
            {
                Point p = new Point(e.TotalManipulation.Translation.X, e.TotalManipulation.Translation.Y);
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

        private static bool IsTap(Point p)
        {
            return Trig.IsTooSmallSwipe(p);
        }

        // ugly, just for now to make it work
        private bool IsWithinBounds(ManipulationCompletedEventArgs e)
        {
            int height = (_settings.SquareSize.Height + _settings.LineWidth.X) * _settings.BoardSize.Height + 3;
            int width = (_settings.SquareSize.Width + _settings.LineWidth.X) * _settings.BoardSize.Width + 3;

            var fwe = e.ManipulationContainer as FrameworkElement;
            if (fwe != null)
            {
                //string cName = e.ManipulationContainer.GetType().Name;
                //bool validControl = cName.Equals("Grid");
                //bool validControl = fwe.Name.Equals(DahlexContent.Name) || fwe.Name.Equals("imgProfessor");
                Point p = new Point(e.ManipulationOrigin.X, e.ManipulationOrigin.Y);
                //Debug.WriteLine(cName + " - " + fwe.Name + " " + p.ToString());

                if (fwe.Name.Equals(DahlexContent.Name))
                {
                    return 0 < p.Y
                        && p.Y <= height
                        && 0 < p.X
                        && p.X <= width;
                }

                if (fwe.Name.Equals("imgProfessor") || fwe.Name.StartsWith("imgHeap") || fwe.Name.StartsWith("imgRobot"))
                {
                    var trans = fwe.TransformToVisual(DahlexContent);
                    Point p2 = trans.Transform(p);

                    return 0 < p2.Y
                        && p2.Y <= height
                        && 0 < p2.X
                        && p2.X <= width;
                }
            }
            return false;
        }

        private TimeSpan _elapsed = TimeSpan.Zero;

        private void timer_tick(object state, EventArgs e)
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

        private DispatcherTimer _gameTimer;

        private void btnStartGame_Click(object sender, RoutedEventArgs e)
        {
            StartGame(GameMode.Random);
        }

        private void btnStartCampaign_Click(object sender, RoutedEventArgs e)
        {
            StartGame(GameMode.Campaign);
        }

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

        private void vibrate()
        {
            VibrateController.Default.Start(new TimeSpan(0, 0, 0, 0, 400));
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

                        vibrate();
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

            int oLeft1 = (pos.X + 1) * (_settings.SquareSize.Width + gridPenWidth) - (32);
            int oTop1 = (pos.Y + 1) * (_settings.SquareSize.Height + gridPenWidth) - (32);

            int oLeft2 = (pos.X) * (_settings.SquareSize.Width + gridPenWidth) - (32);
            int oTop2 = (pos.Y) * (_settings.SquareSize.Height + gridPenWidth) - (32);

            int w3 = (_settings.SquareSize.Width - 4) * 3;
            int h3 = (_settings.SquareSize.Height - 4) * 3;

            const int aniDur = 550;

            storyBomb.Stop();
            storyBomb.Children.Clear();

            ellipseBomb.Visibility = Visibility.Visible;

            // Width

            var daWidth = new DoubleAnimation();
            daWidth.From = 10;
            daWidth.To = w3;

            daWidth.AutoReverse = true;
            daWidth.RepeatBehavior = new RepeatBehavior(1);
            daWidth.FillBehavior = FillBehavior.HoldEnd;

            daWidth.Duration = new Duration(new TimeSpan(0, 0, 0, 0, aniDur));

            Storyboard.SetTargetName(daWidth, ellipseBomb.Name);
            Storyboard.SetTargetProperty(daWidth, new PropertyPath("Width"));

            storyBomb.Children.Add(daWidth);

            // Height

            var daHeight = new DoubleAnimation();
            daHeight.From = 10;
            daHeight.To = h3;

            daHeight.AutoReverse = true;
            daHeight.RepeatBehavior = new RepeatBehavior(1);
            daHeight.FillBehavior = FillBehavior.HoldEnd;

            daHeight.Duration = new Duration(new TimeSpan(0, 0, 0, 0, aniDur));

            Storyboard.SetTargetName(daHeight, ellipseBomb.Name);
            Storyboard.SetTargetProperty(daHeight, new PropertyPath("Height"));

            storyBomb.Children.Add(daHeight);

            // Top

            var daTop = new DoubleAnimation();
            daTop.From = oTop1 + 10;
            daTop.To = oTop2;

            daTop.AutoReverse = true;
            daTop.RepeatBehavior = new RepeatBehavior(1);
            daTop.FillBehavior = FillBehavior.HoldEnd;

            daTop.Duration = new Duration(new TimeSpan(0, 0, 0, 0, aniDur));

            Storyboard.SetTargetName(daTop, ellipseBomb.Name);
            Storyboard.SetTargetProperty(daTop, new PropertyPath("(Canvas.Top)"));

            storyBomb.Children.Add(daTop);

            // Left

            var daLeft = new DoubleAnimation();
            daLeft.From = oLeft1 + 10;
            daLeft.To = oLeft2;

            daLeft.AutoReverse = true;
            daLeft.RepeatBehavior = new RepeatBehavior(1);
            daLeft.FillBehavior = FillBehavior.HoldEnd;

            daLeft.Duration = new Duration(new TimeSpan(0, 0, 0, 0, aniDur));
            daLeft.Completed += daLeft_Completed;

            Storyboard.SetTargetName(daLeft, ellipseBomb.Name);
            Storyboard.SetTargetProperty(daLeft, new PropertyPath("(Canvas.Left)"));

            storyBomb.Children.Add(daLeft);
            storyBomb.Begin();
        }

        private void daLeft_Completed(object sender, EventArgs e)
        {
            ellipseBomb.Visibility = Visibility.Collapsed;
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
                // never happens
                //               AddLineToLog("You won");
                //             btnBomb.IsEnabled = false;
                //           btnTeleport.IsEnabled = false;
                //         _dg.AddHighScore();
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

        private Popup _p;

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
            _p.VerticalOffset = 150;
            _p.HorizontalOffset = 50;

            //p.MinHeight = 200;
            //p.MinWidth = 300;

            // Open the popup.
            _p.IsOpen = true;
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            _p.IsOpen = false;
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
                        var assembly = Assembly.GetExecutingAssembly().FullName;
                        string ver = assembly.Split('=')[1].Split(',')[0];

                        //   textMessage.FontWeight = FontWeights.Normal;
                        textMessage.Text = "Start First Game";
                        //_settings.IsFirstRun = false;
                        //MessageBox.Show("Swipe to move the professor", "First time run advice", MessageBoxButton.OK);
                        popItUp("Welcome to Dahlex v" + ver,
                                "Now on WP8!" + Environment.NewLine +
                                "Enjoy!");
                    }
                    catch
                    { /* safety try */ }
                }
                else
                {
                    // textMessage.FontWeight = FontWeights.Normal;
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
                    //     textMessage.FontWeight = FontWeights.Normal;
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
                //   textMessage.FontWeight = FontWeights.Bold;
                textMessage.Text = "Game Over!";
                hyperMessage1.Content = "New Random Game";
                hyperMessage2.Content = "Start Tutorial";

                FadeIt(cnvMessage, FadeMode.FadeInDelayed, 0, targetOpacity);
            }

            if (gameStatus == GameStatus.GameWon)
            {
                // textMessage.FontWeight = FontWeights.Bold;
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

        private enum FadeMode { None, FadeIn, FadeInDelayed, Hide };

        private FadeMode _prevFadeMode = FadeMode.None;

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
            Storyboard.SetTargetProperty(opan, new PropertyPath("Opacity"));

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

        private void opan_CollapseOnCompleted(object sender, EventArgs e)
        {
            cnvMessage.Visibility = Visibility.Collapsed;
        }

        private void opan_ClickableOnCompleted(object sender, EventArgs e)
        {
            hyperMessage1.Click += hyperMessage1_Click;
            hyperMessage2.Click += hyperMessage2_Click;
        }

        private void hyperMessage_Click(object sender, RoutedEventArgs e)
        {
            // dummy handler
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
                // never happens
                // btnStartGame_Click(sender, e);
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
                        var theUri = new Uri("/Views/ScoresPage.xaml", UriKind.Relative);
                        NavigationService.Navigate(theUri);
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
                //   btnStartCampaign_Click(sender, e);
                // never happens

                //  NavigationService.Navigate()
            }
        }

        public void AddLineToLog(string log)
        {
            log += string.Format(" RC:{0} PC:{1} Img:{2}", storyPanel.Children.Count, storyProf.Children.Count, cnvMovement.Children.Count);

            GameLogger.AddLineToLog(log);
        }

        IDictionary<Guid, Storyboard> _boards = new Dictionary<Guid, Storyboard>();

        public void Animate(BoardPosition bp, IntPoint oldPos, IntPoint newPos, Guid roundId)
        {
            int xOffset = _settings.ImageOffset.X;
            int yOffset = _settings.ImageOffset.Y;
            int gridPenWidth = _settings.LineWidth.X;

            int oLeft = oldPos.X * (_settings.SquareSize.Width + gridPenWidth) + xOffset;
            int oTop = oldPos.Y * (_settings.SquareSize.Height + gridPenWidth) + yOffset;

            int nLeft = newPos.X * (_settings.SquareSize.Width + gridPenWidth) + xOffset;
            int nTop = newPos.Y * (_settings.SquareSize.Height + gridPenWidth) + yOffset;

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

                DoubleAnimation aniL = new DoubleAnimation();
                aniL.AutoReverse = aniProfLeft.AutoReverse;
                aniL.Duration = new Duration(TimeSpan.FromMilliseconds(1400));
                aniL.RepeatBehavior = new RepeatBehavior(1);
                aniL.FillBehavior = FillBehavior.HoldEnd;
                Storyboard.SetTargetName(aniL, bp.ImageName);
                Storyboard.SetTargetProperty(aniL, new PropertyPath("(Canvas.Left)"));

                toDie.Children.Add(aniL);

                aniL.From = oLeft;
                aniL.To = nLeft;

                DoubleAnimation aniT = new DoubleAnimation();
                aniT.AutoReverse = aniProfTop.AutoReverse;
                aniT.Duration = new Duration(TimeSpan.FromMilliseconds(1400));
                aniT.RepeatBehavior = new RepeatBehavior(1);
                aniT.FillBehavior = FillBehavior.HoldEnd;
                Storyboard.SetTargetName(aniT, bp.ImageName);
                Storyboard.SetTargetProperty(aniT, new PropertyPath("(Canvas.Top)"));

                toDie.Children.Add(aniT);

                aniT.From = oTop;
                aniT.To = nTop;

                aniT.Completed += ani_Completed;
                aniL.Completed += ani_Completed;
                AddLineToLog(string.Format("AK {0}", bp.ImageName));

                //toDie.Begin();
            }
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

        private void ani_Completed(object sender, EventArgs e)
        {
            var ani = (DoubleAnimation)sender;
            ani.Completed -= ani_Completed;

            ani.From = ani.To;
            string name = Storyboard.GetTargetName(ani);

            RemoveLines();
            AddLineToLog(string.Format("DisA {0} {1} ", ani.From, name));
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

        public void Clear(bool all)
        {
            var drawer = new BoardDrawing(cnvMovement, storyFade, _settings);

            drawer.Clear(all);
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

        public void RemoveImage(string imageName)
        {
            var drawer = new BoardDrawing(cnvMovement, storyFade, _settings);
            drawer.RemoveImage(imageName);
        }

        private void btnBomb_Click(object sender, RoutedEventArgs e)
        {
            BlowBomb();
        }

        private void btnTeleport_Click(object sender, RoutedEventArgs e)
        {
            DoTeleport();
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

        public void PlaySound(Sound effect)
        {
            if (!_settings.LessSound)
            {
                FrameworkDispatcher.Update();

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

        /// <summary>
        /// http://soundbible.com/1151-Grenade.html
        /// </summary>
        private void PlayBombSoundEffect()
        {
            const string bombMp3 = "Sounds/bomb.wav";
            SoundEffect effect = SoundEffect.FromStream(TitleContainer.OpenStream(bombMp3));
            effect.Play();
        }

        /// <summary>
        /// http://soundbible.com/830-Door-Unlock.html
        /// </summary>
        private void PlayCrashSoundEffect()
        {
            const string crashMp3 = "Sounds/heap.wav";
            SoundEffect effect = SoundEffect.FromStream(TitleContainer.OpenStream(crashMp3));
            effect.Play();
        }

        /// <summary>
        /// http://soundbible.com/709-Bottle-Rocket.html
        /// </summary>
        private void PlayTeleportSoundEffect()
        {
            const string teleMp3 = "Sounds/tele.wav";
            SoundEffect effect = SoundEffect.FromStream(TitleContainer.OpenStream(teleMp3));
            effect.Play();
        }
    }
}