using System.Collections.Generic;

namespace SteidanPrime.Repositories.Saveboard;

public interface ISaveboardRepository
{
    void UpdateSaveChannels(Dictionary<ulong, ulong> saveChannels);

    Dictionary<ulong, ulong> GetSaveChannels();
}