using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace SteidanPrime.Sokoban
{
    class Player
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int OriginalX { get; set; }
        public int OriginalY { get; set; }
        public Grid Grid { get; set; }

        public Player(int X, int Y, Grid Grid)
        {
            this.X = X;
            this.Y = Y;
            this.OriginalX = X;
            this.OriginalY = Y;
            this.Grid = Grid;
        }

        public void SetPosition(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public bool MoveUp()
        {
            if (Grid.IsWall(X, Y - 1)) return false;

            if (Grid.IsBox(X, Y - 1))
            {
                if (Grid.GetBoxAt(X, Y - 1).MoveUp())
                {
                    Y -= 1;
                    return true;
                }

                return false;
            }

            Y -= 1;
            return true;

        }

        public bool MoveRight()
        {
            if (Grid.IsWall(X + 1, Y)) return false;

            if (Grid.IsBox(X + 1, Y))
            {
                if (Grid.GetBoxAt(X + 1, Y).MoveRight())
                {
                    X += 1;
                    return true;
                }

                return false;
            }

            X += 1;
            return true;

        }

        public bool MoveDown()
        {
            if (Grid.IsWall(X, Y + 1)) return false;

            if (Grid.IsBox(X, Y + 1))
            {
                if (Grid.GetBoxAt(X, Y + 1).MoveDown())
                {
                    Y += 1;
                    return true;
                }

                return false;
            }

            Y += 1;
            return true;

        }

        public bool MoveLeft()
        {
            if (Grid.IsWall(X - 1, Y)) return false;

            if (Grid.IsBox(X - 1, Y))
            {
                if (Grid.GetBoxAt(X - 1, Y).MoveLeft())
                {
                    X -= 1;
                    return true;
                }

                return false;
            }

            X -= 1;
            return true;

        }

        public void Reset()
        {
            X = OriginalX;
            Y = OriginalY;
        }
    }
}
