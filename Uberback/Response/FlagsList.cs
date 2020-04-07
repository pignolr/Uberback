using Newtonsoft.Json;

namespace Uberback.Response
{
    public class FlagsList
    {
        [JsonProperty]
        public string[] FlagsImage;

        [JsonProperty]
        public string[] FlagsText;
    }
}
