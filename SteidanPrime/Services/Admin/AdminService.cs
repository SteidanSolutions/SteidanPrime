using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteidanPrime.Services.Admin
{
    public class AdminService
    {
        private readonly DiscordSocketClient _client;
        public AdminService(DiscordSocketClient client)
        {
            _client = client;
        }

        public Task<string> GetAllServers()
        {
            var retVal = "";
            foreach(var guild in _client.Guilds)
                retVal += $"{guild.Id} {guild.Name} {guild.Description}\n";
            return Task.FromResult(retVal);
        }

        public Task<SocketGuild> GetGuild(ulong guildId) => Task.FromResult(_client.GetGuild(guildId));

        public Task<SocketTextChannel> GetTextChannel(SocketGuild guild, ulong channelId) => Task.FromResult(guild.GetTextChannel(channelId));
    }
}
