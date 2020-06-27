using System;
using System.Collections.Generic;
using System.Text;

namespace SteidanPrime.Sokoban
{
    class Destination
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Grid Grid { get; set; }

        public Destination(int X, int Y, Grid Grid)
        {
            this.X = X;
            this.Y = Y;
            this.Grid = Grid;
        }

        public Boolean HasBox(Grid Grid)
        {
            return Grid.IsWall(X, Y);
        }
    }
}
