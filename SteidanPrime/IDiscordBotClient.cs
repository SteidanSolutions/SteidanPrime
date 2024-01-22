using System.Threading.Tasks;

namespace SteidanPrime;

public interface IDiscordBotClient
{
    Task RunAsync();
}