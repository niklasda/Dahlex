using System.Windows.Media.Imaging;
using Dahlex.Logic.Contracts;

namespace Dahlex.Logic.Game
{
    public class BoardImage
    {
        private readonly BitmapImage _bm;

        public BoardImage(BitmapImage bitmap, PieceType type)
        {
            _bm = bitmap;
        }

        public BitmapImage PositionImage
        {
            get { return _bm; }
        }
    }
}