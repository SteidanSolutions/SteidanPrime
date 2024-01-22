using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using SteidanPrime.Commands.Gambling.Blackjack;
using SteidanPrime.Models.Gambling;

namespace SteidanPrime.Services.Gambling;

public interface IGamblingService
{
    Task<(Embed, Result)> NewBlackjackGame(ulong userId, double bet);
    Task<(Embed, Result)> ForfeitBlackjackGame(ulong userId);
    Task<(Embed, Result)> PlayerHit(ulong userId);
    Task<(Embed, Result)> PlayerDoubledDown(ulong userId);
    void SerializePlayers();
    void DeserializePlayers();

    Dictionary<ulong, Player> GetPlayers();
}