using System;
using System.Collections.Generic;
using System.Linq;

namespace SteidanPrime.Services.Gambling.Blackjack
{

    public class Deck
    {
        public List<int> Cards { get; set; } = new List<int>();

        public Deck()
        {
            for (var i = 0; i < 4; i++)
                for (var j = 1; j < 15; j++)
                    Cards.Add(j);
            Cards.RemoveAll(x => x == 1);
        }

        public void ShuffleDeck()
        {
            var rng = new Random();
            Cards = Cards.OrderBy(x => rng.Next()).ToList();
        }

        public int DrawCard()
        {
            var card = Cards[0];
            Cards.Remove(card);
            return card;
        }
    }
}
