using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Dahlex.Logic.Contracts;
using Dahlex.Logic.Settings;
using Dahlex.Logic.Utils;

namespace Dahlex.Logic.Game
{
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

        private Image findImageInCanvas(Canvas cnv, string name)
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

        private void removeImage(string imgName)
        {
            var image = findImageInCanvas(_cnvMovement, imgName);

            if (image != null)
            {
                image.Visibility = Visibility.Collapsed;
            }
        }

        private Image addImage(string imgName, BoardImage boardImage, IntPoint pt, BoardPosition cp)
        {
            var piece = new Image();
            piece.Name = imgName;

            piece.SetValue(Canvas.TopProperty, (double)pt.Y);
            piece.SetValue(Canvas.LeftProperty, (double)pt.X);
            piece.Source = boardImage.PositionImage;

            if (cp.Type == PieceType.Professor)
            {
                var image = findImageInCanvas(_cnvMovement, imgName);

                if (image == null)
                {
                    _cnvMovement.Children.Add(piece);
                }
            }
            else if (cp.Type == PieceType.Heap)
            {
                var image = findImageInCanvas(_cnvMovement, imgName);
                if (image == null)
                {
                    _cnvMovement.Children.Add(piece);
                }
            }
            else
            {
                var image = findImageInCanvas(_cnvMovement, imgName);
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
            Storyboard.SetTargetProperty(opan, new PropertyPath("Opacity"));

            _story.Children.Add(opan);
        }

        public void DrawBoard(IBoard board, int xSize, int ySize)
        {
            _story.Stop();
            _story.Children.Clear();

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
                        BoardImage boardImage = null;
                        int oLeft = x * (xSize + gridPenWidth) + xOffset;
                        int oTop = y * (ySize + gridPenWidth) + yOffset;

                        var pt = new IntPoint(oLeft, oTop);

                        string imgName;
                        if (cp.Type == PieceType.Heap)
                        {
                            imgName = cp.ImageName;
                            BitmapImage pic = LoadImage("heap_02.png");

                            boardImage = new BoardImage(pic, cp.Type);
                            Image img = addImage(imgName, boardImage, pt, cp);
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

                            boardImage = new BoardImage(pic, cp.Type);
                            addImage(imgName, boardImage, pt, cp);
                        }
                        else if (cp.Type == PieceType.Robot)
                        {
                            imgName = cp.ImageName;
                            string name = Randomizer.GetRandomFromSet("robot_05.png", "robot_06.png");
                            BitmapImage pic = LoadImage(name);

                            pic.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                            boardImage = new BoardImage(pic, cp.Type);
                            addImage(imgName, boardImage, pt, cp);
                        }
                        else if (cp.Type == PieceType.None)
                        {
                            imgName = cp.ImageName;
                            removeImage(imgName);
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
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resName = "Dahlex.Logic.Images." + relativeUriString;
            using (Stream str = assembly.GetManifestResourceStream(resName))
            {
                var bi = new BitmapImage();
                bi.SetSource(str);

                return bi;
            }
        }

        public void RemoveImage(string imageName)
        {
            removeImage(imageName);
        }
    }
}