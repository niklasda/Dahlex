using System;
using Dahlex.Logic.Contracts;
using Windows.Foundation;

namespace Dahlex.Logic.Utils
{
    public static class Trig
    {
        private const int DeltaLimit = 42;

        public static bool IsTooSmallSwipe(Point p)
        {
            return Math.Abs(p.X) <= DeltaLimit 
                && Math.Abs(p.Y) <= DeltaLimit;
        }
       
        public static MoveDirection GetSwipeDirection(Point p)
        {
            return IsTooSmallSwipe(p) ? MoveDirection.Ignore : GetTranslationDirection(p);
        }

        private static MoveDirection GetTranslationDirection(Point p)
        {

            var angle = Math.Atan2(p.Y, p.X);

            if (-Math.PI / 8 <= angle && angle < Math.PI / 8)
            {
                return MoveDirection.East;
            }
            if (Math.PI / 8 <= angle && angle < 3 * Math.PI / 8)
            {
                return MoveDirection.SouthEast;
            }
            if (3 * Math.PI / 8 <= angle && angle < 5 * Math.PI / 8)
            {
                return MoveDirection.South;
            }
            if (5 * Math.PI / 8 <= angle && angle < 7 * Math.PI / 8)
            {
                return MoveDirection.SouthWest;
            }
            if (7 * Math.PI / 8 <= angle && angle < Math.PI)
            {
                return MoveDirection.West;
            }
            if (-Math.PI<= angle && angle < -7*Math.PI/8)
            {
                return MoveDirection.West;
            }
            if (-7 * Math.PI / 8 <= angle && angle < -5 * Math.PI / 8)
            {
                return MoveDirection.NorthWest;
            }
            if (-5 * Math.PI / 8 <= angle && angle < -3 * Math.PI / 8)
            {
                return MoveDirection.North;
            }
            if (-3 * Math.PI / 8 <= angle && angle < - Math.PI / 8)
            {
                return MoveDirection.NorthEast;
            }

            return MoveDirection.Ignore;
        }
    }
}
