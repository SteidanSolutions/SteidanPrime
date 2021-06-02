using System;
using System.Collections.Generic;
using System.Text;

namespace SteidanPrime.Sokoban
{
    internal class Box
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


        public bool IsDestination()
        {
            return Grid.IsDestination(X, Y);
        }

        public void Reset()
        {
            X = OriginalX;
            Y = OriginalY;
        }

        public bool MoveUp()
        {
            if (!Grid.IsWall(X, Y - 1) && !Grid.IsBox(X, Y - 1))
            {
                Y -= 1;
                return true;
            }

            return false;
        }

        public bool MoveRight()
        {
            if (!Grid.IsWall(X + 1, Y) && !Grid.IsBox(X + 1, Y))
            {
                X += 1;
                return true;
            }

            return false;
        }

        public bool MoveDown()
        {
            if (!Grid.IsWall(X, Y + 1) && !Grid.IsBox(X, Y + 1))
            {
                Y += 1;
                return true;
            }

            return false;
        }

        public bool MoveLeft()
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
