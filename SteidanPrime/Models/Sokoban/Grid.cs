using System;
using System.Collections.Generic;
using System.Linq;

namespace SteidanPrime.Models.Sokoban
{
    public class Grid
    {
        private const int MAX_BOXES = 8;

        public int BoxCount { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Color { get; set; }
        public Tile[,] CurrentGrid { get; set; }
        public Player Player { get; set; }
        public List<Box> Boxes { get; set; }
        public List<Destination> Destinations { get; set; }

        readonly Random _rand = new Random();

        public Grid(int width, int height, int boxCount)
        {
            Player = new Player(_rand.Next(1, width - 1), _rand.Next(1, height - 1), this);
            BoxCount = boxCount > MAX_BOXES ? MAX_BOXES : boxCount;

            Height = height;
            Width = width;
            CurrentGrid = new Tile[width, height];

            Boxes = new List<Box>();
            Destinations = new List<Destination>();

            GenerateBoxes();
            GenerateDestinations();
            UpdateGrid();
        }

        public bool IsWall(int X, int Y)
        {
            return CurrentGrid[X, Y].Status == StatusType.WALL;
        }

        public bool IsBox(int X, int Y)
        {
            return CurrentGrid[X, Y].Status == StatusType.BOX;
        }

        public bool IsDestination(int X, int Y)
        {
            return CurrentGrid[X, Y].Status == StatusType.DESTINATION;
        }

        public Box GetBoxAt(int X, int Y)
        {
            return Boxes.FirstOrDefault(Box => X == Box.X && Y == Box.Y);
        }

        public bool IsBoxRaw(int X, int Y)
        {
            return Boxes.Any(Box => X == Box.X && Y == Box.Y);
        }

        public void GenerateBoxes()
        {
            Color = _rand.Next(6);

            for (int i = 0; i < BoxCount; i++)
            {
                int X = _rand.Next(2, Width - 2);
                int Y = _rand.Next(2, Height - 2);

                foreach (var box in Boxes)
                {
                    while (X == box.X && Y == box.Y || (X == Player.X && Y == Player.Y))
                    {
                        X = _rand.Next(2, Width - 2);
                        Y = _rand.Next(2, Height - 2);
                    }
                }

                Boxes.Add(new Box(X, Y, this));
            }
        }

        public void GenerateDestinations()
        {
            for (int i = 0; i < BoxCount; i++)
            {
                int X = _rand.Next(1, Width - 1);
                int Y = _rand.Next(1, Height - 1);

                foreach (var destination in Destinations)
                {
                    while (X == destination.X && Y == destination.Y || IsBoxRaw(X, Y) || (X == Player.X && Y == Player.Y))
                    {
                        X = _rand.Next(1, Width - 1);
                        Y = _rand.Next(1, Height - 1);
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

                    foreach (var destination in Destinations.Where(destination => destination.X == X && destination.Y == Y))
                        CurrentGrid[X, Y] = new Tile(StatusType.DESTINATION);

                    if (Player.X == X && Player.Y == Y)
                        CurrentGrid[X, Y] = new Tile(StatusType.PLAYER);

                    foreach (var box in Boxes.Where(box => box.X == X && box.Y == Y))
                    {
                        if (box.IsDestination())
                            CurrentGrid[X, Y] = new Tile(Color, StatusType.WALL);
                        else
                            CurrentGrid[X, Y] = new Tile(StatusType.BOX);
                    }
                }
            }
        }

        public bool HasWon()
        {
            return Destinations.All(destination => destination.HasBox(this));
        }

        public void Reset()
        {
            Player.Reset();

            foreach (var box in Boxes)
                box.Reset();

            UpdateGrid();
        }

        public override string ToString()
        {
            UpdateGrid();
            string result = "";

            for (int Y = 0; Y < Height; Y++)
            {
                for (int X = 0; X < Width; X++)
                    result += CurrentGrid[X, Y];

                result += "\n";
            }

            return result;
        }
    }
}
