using Newtonsoft.Json;
using System.Collections.Generic;

namespace Uberback.Response
{
    public class Collect
    {
        [JsonProperty]
        public Data[] Data;

        [JsonProperty]
        public Dictionary<string, double> FlagsPercentage;
    }
}
