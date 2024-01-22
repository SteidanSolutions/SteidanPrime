using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace SteidanPrime.Services.Sokoban;

public interface ISokobanService
{
   Task SokobanButtonHandler(SocketMessageComponent component);
   Task<(Embed, bool)> NewGameAsync(ulong userId);
   Task<Embed> StopGame(ulong userId);
   Task<(Embed, bool)> ContinueGame(ulong userId);
   Task<(Embed, bool)> GetGameEmbed(ulong userId);
}