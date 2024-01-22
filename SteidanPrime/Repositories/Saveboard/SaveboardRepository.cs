using System.Collections.Generic;

namespace SteidanPrime.Repositories.Saveboard;

public class SaveboardRepository : ISaveboardRepository
{
    private Dictionary<ulong, ulong> _saveChannels { get; set; } // TODO change to Redis
    public void UpdateSaveChannels(Dictionary<ulong, ulong> saveChannels)
    {
        _saveChannels = saveChannels;
    }

    public Dictionary<ulong, ulong> GetSaveChannels()
    {
        return _saveChannels;
    }
}