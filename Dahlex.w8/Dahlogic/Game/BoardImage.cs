using System.Windows.Media.Imaging;
using Dahlex.Logic.Contracts;

namespace Dahlex.Logic.Game
{
    public class BoardImage
    {
        private readonly BitmapImage _bm;
        //   private int? bmIndex;
        //private PieceType _pType;   // might be removable

        public BoardImage(BitmapImage bitmap, PieceType type)
        {
            _bm = bitmap;
        //    _pType = type;
        }

        public BitmapImage PositionImage
        {
            get { return _bm; }
        }
    }
}