using System;

namespace SteidanPrime.Models.Gambling
{
    public class Player
    {
        public ulong Id { get; set; }
        public double VergilBucks { get; set; } = 500;
        public int Blackjacks { get; set; } = 0;
        public int BlackjackWins { get; set; } = 0;
        public int BlackjackLosses { get; set; } = 0;
        public int BlackjackStandOffs { get; set; } = 0;
        public int BlackjackPushes { get; set; } = 0;
        public bool CurrentlyPlayingBlackjack { get; set; } = false;
        public long LastPaydayTime { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
