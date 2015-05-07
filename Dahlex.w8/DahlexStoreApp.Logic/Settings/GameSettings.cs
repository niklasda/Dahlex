using System.Runtime.Serialization;
using Dahlex.Logic.Contracts;

namespace Dahlex.Logic.Settings
{
    //[DataContract]
    public class GameSettings
    {
        private IntSize _canvasSize;
        public GameSettings(IntSize canvasSize)
        {
            _canvasSize = canvasSize;
        }
     //   [DataMember]
        public string PlayerName;

     //   [DataMember]
        public bool LessSound;

        /// <summary>
        /// Number of squares on the board
        /// </summary>
        //   [IgnoreDataMember]
        public IntSize BoardSize
        {
            get
            {
                int h = (int)(_canvasSize.Height / SquareSize.Height);
                int w = (int)(_canvasSize.Width / SquareSize.Width); 
                return new IntSize(w, h);
            }
        }

        // w, h

        /// <summary>
        /// The size of the squares on the board, TODO with or without margin ???
        /// </summary>
     //   [IgnoreDataMember]
        public readonly IntSize SquareSize = new IntSize(42, 42); // image size 42 x 42

        /// <summary>
        /// The offset to apply to get the images inside the squares
        /// </summary>
    //    [IgnoreDataMember]
        public readonly IntPoint ImageOffset = new IntPoint(1, 1); // w, h

        /// <summary>
        /// The distance between squares
        /// </summary>
    //    [IgnoreDataMember]
        public readonly IntPoint LineWidth = new IntPoint(0, 0);

        //[IgnoreDataMember]
        //public bool IsFirstRun;
    //    [IgnoreDataMember]
        public int MaxNumberOfLevel
        {
            get { return (BoardSize.Width * BoardSize.Height) / 4 + 10; }
        }
    }
}