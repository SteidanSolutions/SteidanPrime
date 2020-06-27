using Discord;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SteidanPrime.Sokoban
{
    class Grid
    {
        private readonly int MAX_BOXES = 8;

        public int BoxCount { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Color { get; set; }
        public Tile[,] CurrentGrid { get; set; }
        public Player Player { get; set; }
        public List<Box> Boxes { get; set; }
        public List<Destination> Destinations { get; set; }

        readonly Random rand = new Random();

        public Grid(int Width, int Height, int BoxCount)
        {
            Player = new Player(rand.Next(1, Width - 1), rand.Next(1, Height - 1), this);
            if (BoxCount > MAX_BOXES)
                this.BoxCount = MAX_BOXES;
            else
                this.BoxCount = BoxCount;

            this.Height = Height;
            this.Width = Width;
            CurrentGrid = new Tile[Width, Height];

            Boxes = new List<Box>();
            Destinations = new List<Destination>();

            GenerateBoxes();
            GenerateDestinations();
            UpdateGrid();
        }

        public Boolean IsWall(int X, int Y)
        {
            return CurrentGrid[X, Y].Status == StatusType.WALL;
        }

        public Boolean IsBox(int X, int Y)
        {
            return CurrentGrid[X, Y].Status == StatusType.BOX;
        }

        public Boolean IsDestination(int X, int Y)
        {
            return CurrentGrid[X, Y].Status == StatusType.DESTINATION;
        }

        public Box GetBoxAt(int X, int Y)
        {
            foreach (var Box in Boxes)
                if (X == Box.X && Y == Box.Y)
                    return Box;

            return null;
        }

        public Boolean IsBoxRaw(int X, int Y)
        {
            foreach (var Box in Boxes)
                if (X == Box.X && Y == Box.Y)
                    return true;

            return false;
        }

        public void GenerateBoxes()
        {
            this.Color = rand.Next(6);

            for (int i = 0; i < BoxCount; i++)
            {
                int X = rand.Next(2, Width - 2);
                int Y = rand.Next(2, Height - 2);

                foreach (var Box in Boxes)
                {
                    while (X == Box.X && Y == Box.Y || (X == Player.X && Y == Player.Y))
                    {
                        X = rand.Next(2, Width - 2);
                        Y = rand.Next(2, Height - 2);
                    }
                }

                Boxes.Add(new Box(X, Y, this));
            }
        }

        public void GenerateDestinations()
        {
            for (int i = 0; i < BoxCount; i++)
            {
                int X = rand.Next(1, Width - 1);
                int Y = rand.Next(1, Height - 1);

                foreach (var Destination in Destinations)
                {
                    while (X == Destination.X && Y == Destination.Y || IsBoxRaw(X, Y) || (X == Player.X && Y == Player.Y))
                    {
                        X = rand.Next(1, Width - 1);
                        Y = rand.Next(1, Height - 1);
                    }
                }

                Destinations.Add(new Destination(X, Y, this));
            }
        }

        public void UpdateGrid()
        {
            for (int X = 0; X < Width; X++)
            {
                for (int Y = 0; Y < Height; Y++)
                { 
                    CurrentGrid[X, Y] = new Tile(StatusType.GROUND);

                    if (X == 0 || X == Width - 1 || Y == 0 || Y == Height - 1)
                        CurrentGrid[X, Y] = new Tile(Color, StatusType.WALL);

                    foreach (var Destination in Destinations)
                        if (Destination.X == X && Destination.Y == Y)
                            CurrentGrid[X, Y] = new Tile(StatusType.DESTINATION);

                    if (Player.X == X && Player.Y == Y)
                        CurrentGrid[X, Y] = new Tile(StatusType.PLAYER);

                    foreach (var Box in Boxes)
                    {
                        if (Box.X == X && Box.Y == Y)
                        {
                            if (Box.IsDestination())
                                CurrentGrid[X, Y] = new Tile(Color, StatusType.WALL);
                            else
                                CurrentGrid[X, Y] = new Tile(StatusType.BOX);
                        }
                    }
                }
            }
        }

        public Boolean HasWon()
        {
            foreach (var Destination in Destinations)
                if (!Destination.HasBox(this))
                    return false;

            return true;
        }

        public void Reset()
        {
            Player.Reset();

            foreach (var Box in Boxes)
                Box.Reset();

            UpdateGrid();
        }

        public override string ToString()
        {
            UpdateGrid();
            String Result = "";

            for (int Y = 0; Y < Height; Y++)
            {
                for (int X = 0; X < Width; X++)
                    Result += CurrentGrid[X, Y];

                Result += "\n";
            }

            return Result;
        }
    }
}
