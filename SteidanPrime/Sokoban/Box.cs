using System;
using System.Collections.Generic;
using System.Text;

namespace SteidanPrime.Sokoban
{
    class Box
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int OriginalX { get; set; }
        public int OriginalY { get; set; }
        Grid Grid;

        public Box(int X, int Y, Grid Grid)
        {
            this.X = X;
            this.Y = Y;
            this.OriginalX = X;
            this.OriginalY = Y;
            this.Grid = Grid;
        }


        public Boolean IsDestination()
        {
            return Grid.IsDestination(X, Y);
        }

        public void Reset()
        {
            X = OriginalX;
            Y = OriginalY;
        }

        public Boolean MoveUp()
        {
            if (!Grid.IsWall(X, Y - 1) && !Grid.IsBox(X, Y - 1))
            {
                Y -= 1;
                return true;
            }

            return false;
        }

        public Boolean MoveRight()
        {
            if (!Grid.IsWall(X + 1, Y) && !Grid.IsBox(X + 1, Y))
            {
                X += 1;
                return true;
            }

            return false;
        }

        public Boolean MoveDown()
        {
            if (!Grid.IsWall(X, Y + 1) && !Grid.IsBox(X, Y + 1))
            {
                Y += 1;
                return true;
            }

            return false;
        }

        public Boolean MoveLeft()
        {
            if (!Grid.IsWall(X - 1, Y) && !Grid.IsBox(X - 1, Y))
            {
                X -= 1;
                return true;
            }

            return false;
        }
    }
}
