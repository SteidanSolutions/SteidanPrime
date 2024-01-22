using System.Collections.Generic;

namespace SteidanPrime.Commands.Gambling.Blackjack
{
    public class Grid
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Tile[,] CurrentGrid { get; set; }
        public List<int> PlayerCards { get; set; } = new List<int>();
        public List<int> DealerCards { get; set; } = new List<int>();
        public bool RevealFaceDownCard { get; set; } = false;
        public Deck Deck { get; set; } = new Deck();
        public Grid(int width = 11, int height = 5)
        {
            Width = width;
            Height = height;
            CurrentGrid = new Tile[width, height];
            Deck.ShuffleDeck();
            PlayerCards.Add(Deck.DrawCard());
            DealerCards.Add(Deck.DrawCard());
            PlayerCards.Add(Deck.DrawCard());
            DealerCards.Add(Deck.DrawCard());
            UpdateGrid();
        }

        // TODO move this logic to service/repository
        public void UpdateGrid()
        {
            for (int y = 0; y < Height; y++)
            {
                int cardCounter = 0;
                for (int x = 0; x < Width; x++)
                {
                    if (y == 0 && DealerCards.Count > 0)
                    {
                        int numberOfDealerMiddleTiles = 2 * DealerCards.Count - 1;
                        int dealerMiddleTilesStartAt = (Width / 2 + 1) - numberOfDealerMiddleTiles / 2 - 1;
                        int dealerMiddleTilesEndAt = (Width / 2 + 1) + numberOfDealerMiddleTiles / 2;

                        if (x >= dealerMiddleTilesStartAt && x <= dealerMiddleTilesEndAt)
                        {
                            if ((x - dealerMiddleTilesStartAt)%2 == 0)
                            {
                                if (cardCounter == 1 && !RevealFaceDownCard)
                                    CurrentGrid[x, y] = new Tile(0);
                                else
                                    CurrentGrid[x, y] = new Tile(DealerCards[cardCounter]);
                                cardCounter++;
                            }
                            else
                            {
                                CurrentGrid[x, y] = new Tile(-1);
                            }
                        }
                        else
                        {
                            CurrentGrid[x, y] = new Tile(-1);
                        }
                    }
                    else if (y == Height-1 && PlayerCards.Count > 0)
                    {
                        int numberOfPlayerMiddleTiles = 2 * PlayerCards.Count - 1;
                        int playerMiddleTilesStartAt = (Width / 2 + 1) - numberOfPlayerMiddleTiles / 2 - 1;
                        int playerMiddleTilesEndAt = (Width / 2 + 1) + numberOfPlayerMiddleTiles / 2;

                        if (x >= playerMiddleTilesStartAt && x <= playerMiddleTilesEndAt)
                        {
                            if ((x - playerMiddleTilesStartAt) % 2 == 0)
                            {
                                CurrentGrid[x, y] = new Tile(PlayerCards[cardCounter]);
                                cardCounter++;
                            }
                            else
                            {
                                CurrentGrid[x, y] = new Tile(-1);
                            }
                        }
                        else
                        {
                            CurrentGrid[x, y] = new Tile(-1);
                        }
                    }
                    else
                    {
                        CurrentGrid[x, y] = new Tile(-1);
                        
                    }
                }
            }
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
