using Discord.Commands;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Web;

namespace SteidanPrime
{
    public class TaskHandler : ModuleBase<SocketCommandContext>
    {
        [Command("yourmum")]

        public async Task Mum()
        {
            string endMessage = "Poop";

            await Context.Channel.SendMessageAsync(endMessage);
        }

        [Command("yourmum")]

        public async Task Mum(string mentions)
        {
            //string endMessage = "User does not exist.";
            string endMessage = mentions + " " + "https://cdn.discordapp.com/attachments/608672795723038741/616735947815387171/unknown.png";
            //if (Context.Guild.Users.Where(x => x.Username == mentions.Replace("@", " ")).Any())
            //endMessage = "@" + mentions + " " + "https://cdn.discordapp.com/attachments/608672795723038741/616735947815387171/unknown.png";

            await Context.Channel.SendMessageAsync(endMessage);
        }

        [Command("someone")]
        [Summary("Pings a random person on the server.")]

        public async Task Someone()
        {
            await Context.Channel.SendMessageAsync($"<@{Context.Guild.Users.ElementAt(new Random().Next(0, Context.Guild.Users.Count)).Id}>");
        }
    }
}
