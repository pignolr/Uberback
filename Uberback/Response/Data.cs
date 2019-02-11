using Newtonsoft.Json;

namespace Uberback.Response
{
    public class Data
    {
        [JsonProperty]
        public string DateTime;

        [JsonProperty]
        public string Flags;

        [JsonProperty]
        public string UserId;
    }
}
