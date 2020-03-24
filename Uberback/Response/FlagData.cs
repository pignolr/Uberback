using Newtonsoft.Json;
using System.Collections.Generic;

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
