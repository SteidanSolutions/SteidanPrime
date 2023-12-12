using System.Threading.Tasks;
using Discord.WebSocket;

namespace SteidanPrime.Services.Admin;

public interface IAdminService
{
    Task<string> GetAllServers();

    Task<SocketGuild> GetGuild(ulong guildId);

    Task<SocketTextChannel> GetTextChannel(SocketGuild guild, ulong channelId);
}