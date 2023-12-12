using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SteidanPrime.Repositories.Saveboard;

namespace SteidanPrime.Services.Saveboard
{
    public class SaveboardService : ISaveboardService
    {
        private readonly ISaveboardRepository _saveboardRepository;
        
        public SaveboardService(ISaveboardRepository saveboardRepository)
        {
            _saveboardRepository = saveboardRepository;
            DeserializeSaveboard();
        }
        
        public void SerializeSaveboard()
        {
            var saveChannels = _saveboardRepository.GetSaveChannels();
            File.WriteAllText("Resources/saveboard.json", JsonSerializer.Serialize(saveChannels));
        }

        public void DeserializeSaveboard()
        {

            if (File.Exists("Resources/saveboard.json"))
            {
                var saveChannels = JsonSerializer.Deserialize<Dictionary<ulong, ulong>>(
                    File.ReadAllText("Resources/saveboard.json"));

                _saveboardRepository.UpdateSaveChannels(saveChannels);
            }
        }
    }
}
