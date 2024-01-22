using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace SteidanPrime.Services.Markov;

public interface IMarkovService
{
    Task HandleTextAsync(string message, ulong guildId);
    string GetChain(SocketGuild guild);
    void SerializeDict();
    void DeserializeDict();
    void FixMissingDictionaries();
    ulong GetTotalWords();
    string GetChainWithSpecificWord(SocketGuild guild, string word);
    Dictionary<ulong, Dictionary<string, List<string>>> GetMarkovDict();
}