using Newtonsoft.Json;

namespace Uberback.Response
{
    public class FlagData
    {
        [JsonProperty]
        public string Name;

        [JsonProperty]
        public double Value;

        [JsonProperty]
        public double PercentValue;
    }
}
