using System.Threading.Tasks;

namespace SteidanPrime.Services.Common;

public interface IInteractionHandler
{
    Task InitializeAsync();
}