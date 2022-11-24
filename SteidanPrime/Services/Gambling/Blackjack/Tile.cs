namespace SteidanPrime.Services.Gambling.Blackjack
{
    public class Tile
    {
        public int Card { get; set; }

        public Tile(int card)
        {
            this.Card = card;
        }

        public override string ToString()
        {
            return Card switch
            {
                0 => ":black_joker:",
                1 => ":regional_indicator_a:",
                2 => ":two:",
                3 => ":three:",
                4 => ":four:",
                5 => ":five:",
                6 => ":six:",
                7 => ":seven:",
                8 => ":eight:",
                9 => ":nine:",
                10 => ":keycap_ten:",
                11 => ":regional_indicator_a:",
                12 => ":regional_indicator_j:",
                13 => ":regional_indicator_q:",
                14 => ":regional_indicator_k:",
                _ => ":black_large_square:"
            };
        }
    }
}
