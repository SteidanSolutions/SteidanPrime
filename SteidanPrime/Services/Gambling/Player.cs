using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

namespace SteidanPrime.Services.Gambling
{
    public class Player
    {
        public int VergilBucks { get; set; } = 500;
        public int Blackjacks { get; set; } = 0;
        public int BlackjackWins { get; set; } = 0;
        public int BlackjackLosses { get; set; } = 0;
        public bool CurrentlyPlayingBlackjack { get; set; } = false;
        public long LastPaydayTime { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        public List<int> Hand = new List<int>();

        public Player() {}
    }
}
