using Dahlex.Logic.Contracts;

namespace Dahlex.Logic.Game
{
    public class BoardPosition
    {
        private PieceType _type;
        private string _imageName;
        private bool _isNew;

        public bool IsNew
        {
            get { return _isNew; }
            set { _isNew = value; }
        }

        public PieceType Type
        {
            get { return _type; }
        }

        public string ImageName
        {
            get { return _imageName; }
            set { _imageName = value; }
        }

        public BoardPosition(PieceType pType, string imgName)
        {
            _type = pType;
            _imageName = imgName;
        }

        public static BoardPosition CreateProfessorBoardPosition()
        {
            return new BoardPosition(PieceType.Professor, "imgProfessor");
        }

        public static BoardPosition CreateHeapBoardPosition(int index)
        {
            return new BoardPosition(PieceType.Heap, "imgHeap" + index);
        }

        public static BoardPosition CreateRobotBoardPosition(int index)
        {   
            return new BoardPosition(PieceType.Robot, "imgRobot" + index);
        }
        
        public void ConvertToNone()
        {
            _type = PieceType.None;
        }

        public void ConvertToHeap()
        {
            _type = PieceType.Heap;
            _imageName = "imgHeap" + _imageName;
            _isNew = true;
            //TODO re-imp
        }
    }
}