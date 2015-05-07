using System;
using Dahlex.Logic.Contracts;
using Dahlex.Logic.Settings;
using Dahlex.Logic.Utils;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace Dahlex.Logic.Game
{
    [CLSCompliant(false)]
    public class BoardDrawing
    {
        private readonly Canvas _cnvMovement;
        private readonly Storyboard _story;
        private readonly GameSettings _settings;

        public BoardDrawing(Canvas canvasMovement, Storyboard storyBoard, GameSettings settings)
        {
            _cnvMovement = canvasMovement;
            _story = storyBoard;
            _settings = settings;
        }

        public void Clear(bool all)
        {
            if (all)
            {
                _cnvMovement.Children.Clear();
            }
        }

        private Image FindImageInCanvas(Canvas cnv, string name)
        {
            foreach (UIElement child in cnv.Children)
            {
                var img = child as Image;
                if (img != null && img.Name.Equals(name))
                {
                    return img;
                }
            }

            return null;
        }

        private Image AddImage(string imgName, BitmapImage boardImage, IntPoint pt, BoardPosition cp)
        {
            var piece = new Image();
            piece.Name = imgName;

            piece.SetValue(Canvas.TopProperty, (double)pt.Y);
            piece.SetValue(Canvas.LeftProperty, (double)pt.X);
            piece.Source = boardImage/*.PositionImage*/;

            if (cp.Type == PieceType.Professor)
            {
                var image = FindImageInCanvas(_cnvMovement, imgName);

                if (image == null)
                {
                    _cnvMovement.Children.Add(piece);
                }
            }
            else if (cp.Type == PieceType.Heap)
            {
                var image = FindImageInCanvas(_cnvMovement, imgName);
                if (image == null)
                {
                    _cnvMovement.Children.Add(piece);
                }
            }
            else
            {
                var image = FindImageInCanvas(_cnvMovement, imgName);
                if (image == null)
                {
                    _cnvMovement.Children.Add(piece);
                }
            }

            return piece;
        }

        private void AddToFade(Image piece, double start, double end)
        {
            var easer = new PowerEase();
            easer.EasingMode = EasingMode.EaseIn;
            easer.Power = 20;

            var opan = new DoubleAnimation();
            opan.From = start;
            opan.To = end;

            opan.AutoReverse = false;
            opan.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 1000));
            opan.RepeatBehavior = new RepeatBehavior(1);
            opan.FillBehavior = FillBehavior.Stop;

            opan.EasingFunction = easer;
            Storyboard.SetTargetName(opan, piece.Name);
            Storyboard.SetTargetProperty(opan, "Opacity");

            _story.Children.Add(opan);
        }

        public void DrawLines(Canvas cnvLines)
        {
            int h = (int)(_cnvMovement.ActualHeight / _settings.SquareSize.Height);
            int w = (int)(_cnvMovement.ActualWidth / _settings.SquareSize.Width);

            if (cnvLines.Children.Count < 10)
            {
                for (int y = 0; y <= h; y++)
                {
                    cnvLines.Children.Add(new Line()
                    {
                        Name = string.Format("lineY_{0}", y),
                        Width = _cnvMovement.ActualWidth,
                        Height = _cnvMovement.ActualHeight,
                        X1 = 0,
                        X2 = _cnvMovement.ActualWidth,
                        Y1 = y * _settings.SquareSize.Height,
                        Y2 = y * _settings.SquareSize.Height,
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeThickness = 0.5
                    });
                }

                // make sure the bottom edge is painted black
                cnvLines.Children.Add(new Rectangle()
                    {
                        Stroke = new SolidColorBrush(Colors.Black),
                        Width = _cnvMovement.ActualWidth,
                        Height = _settings.SquareSize.Height,
                        Margin = new Thickness(0, h * _settings.SquareSize.Height, 0, 0),
                        StrokeThickness = _settings.SquareSize.Height / 2
                    });

                for (int x = 0; x <= w; x++)
                {
                    cnvLines.Children.Add(new Line()
                    {
                        Name = string.Format("lineX_{0}", x),
                        Width = _cnvMovement.ActualWidth,
                        Height = _cnvMovement.ActualHeight,
                        X1 = x * _settings.SquareSize.Width,
                        X2 = x * _settings.SquareSize.Width,
                        Y1 = 0,
                        Y2 = _cnvMovement.ActualHeight,
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeThickness = 0.5
                    });
                }

                // make sure the right edge is painted black
                cnvLines.Children.Add(new Rectangle()
                {
                    Stroke = new SolidColorBrush(Colors.Black),
                    Width = _settings.SquareSize.Width,
                    Height = _cnvMovement.ActualHeight,
                    Margin = new Thickness(w * _settings.SquareSize.Width, 0, 0, 0),
                    StrokeThickness = _settings.SquareSize.Width / 2
                });

            }
        }

        public void DrawBoard(IBoard board, int xSize, int ySize)
        {
            //   try
            //   {
            _story.Stop();
            _story.Children.Clear();
            //   }
            //   catch (Exception)
            //   {
            // weird com exception sometimes;
            //       var m = new MessageDialog("Problem");
            //       m.ShowAsync();
            //   }


            int xOffset = _settings.ImageOffset.X;
            int yOffset = _settings.ImageOffset.Y;
            int gridPenWidth = _settings.LineWidth.X;

            for (int x = 0; x < board.GetPositionWidth(); x++)
            {
                for (int y = 0; y < board.GetPositionHeight(); y++)
                {
                    BoardPosition cp = board.GetPosition(x, y);
                    if (cp != null)
                    {
                        BitmapImage boardImage = null;
                        int oLeft = x * (xSize + gridPenWidth) + xOffset;
                        int oTop = y * (ySize + gridPenWidth) + yOffset;

                        var pt = new IntPoint(oLeft, oTop);

                        string imgName;
                        if (cp.Type == PieceType.Heap)
                        {
                            imgName = cp.ImageName;
                            BitmapImage pic = LoadImage("heap_02.png");

                            boardImage = pic;
                            Image img = AddImage(imgName, boardImage, pt, cp);
                            if (cp.IsNew)
                            {
                                AddToFade(img, 0, 1);
                                cp.IsNew = false;
                            }
                        }
                        else if (cp.Type == PieceType.Professor)
                        {
                            imgName = cp.ImageName;
                            BitmapImage pic = LoadImage("planet_01.png");

                            boardImage = pic;
                            AddImage(imgName, boardImage, pt, cp);
                        }
                        else if (cp.Type == PieceType.Robot)
                        {
                            imgName = cp.ImageName;
                            string name = Randomizer.GetRandomFromSet("robot_05.png", "robot_06.png");
                            BitmapImage pic = LoadImage(name);

                            pic.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                            boardImage = pic;
                            AddImage(imgName, boardImage, pt, cp);
                        }
                        else if (cp.Type == PieceType.None)
                        {
                            imgName = cp.ImageName;
                            RemoveImage(imgName);
                        }

                        if (boardImage != null)
                        {
                        }
                        else if (cp.Type != PieceType.None)
                        {
                            throw new Exception("Invalid Type of BoardPosition");
                        }
                    }
                }
            }

            _story.Begin();
        }

        private BitmapImage LoadImage(string relativeUriString)
        {
            string resName = string.Format("ms-appx:///Assets/Board/{0}", relativeUriString);

            var bi = new BitmapImage();
            bi.UriSource = new Uri(resName, UriKind.Absolute);

            return bi;
        }

        public void RemoveImage(string imageName)
        {
            var image = FindImageInCanvas(_cnvMovement, imageName);

            if (image != null)
            {
                image.Visibility = Visibility.Collapsed;
            }
        }
    }
}