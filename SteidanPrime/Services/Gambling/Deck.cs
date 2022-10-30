using System.Collections.Generic;

namespace SteidanPrime.Services.Gambling
{

    public class Deck
    {
        public Dictionary<ulong, List<int>> Decks { get; set; }

        public List<int> ShuffleDeck()
        {
            var deck = new List<int>();
            for (var i = 0; i < 4; i++)
                for (var j = 1; i < 15; i++)
                    deck.Add(j);
            deck.RemoveAll(x => x == 11);
            return deck;
        }

        public int DrawCard(ulong userId)
        {
            return Decks[userId][0];
        }
    }
}
