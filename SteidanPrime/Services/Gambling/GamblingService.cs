using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SteidanPrime.Services.Gambling
{
    public class GamblingService
    {
        public Dictionary<ulong, Player> Players { get; set; }


        public GamblingService()
        {
            DeserializePlayers();
        }

        public void SerializePlayers()
        {
            File.WriteAllText("Resources/wallet.json", JsonConvert.SerializeObject(Players));
        }

        public void DeserializePlayers()
        {
            if (File.Exists("Resources/wallet.json"))
                Players = JsonConvert.DeserializeObject<Dictionary<ulong, Player>>(
                    File.ReadAllText("Resources/wallet.json"));
            else
                Players = new Dictionary<ulong, Player>();
        }


    }
}
