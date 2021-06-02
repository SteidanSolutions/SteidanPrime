using Newtonsoft.Json;

namespace SteidanPrime
{
    public enum ApplicationRunningMethod
    {
        SERVICE,
        CONSOLE
    }

    public class Settings
    {
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("prefix")]
        public string CommandPrefix { get; set; }
        public ApplicationRunningMethod ApplicationRunningMethod { get; set; }
    }
}
