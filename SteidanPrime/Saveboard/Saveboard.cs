using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SteidanPrime.Saveboard
{
    class Saveboard
    {
        public Dictionary<ulong, ulong> SaveChannels { get; set; }

        public Saveboard()
        {
            DeserializeSaveboard();
        }

        public void SerializeSaveboard()
        {
            File.WriteAllText("Resources/saveboard.json", JsonConvert.SerializeObject(SaveChannels));
        }

        public void DeserializeSaveboard()
        {
            if (File.Exists("Resources/saveboard.json"))
                SaveChannels =
                    JsonConvert.DeserializeObject<Dictionary<ulong, ulong>>(
                        File.ReadAllText("Resources/saveboard.json"));
            else
                SaveChannels = new Dictionary<ulong, ulong>();
        }
    }
}
