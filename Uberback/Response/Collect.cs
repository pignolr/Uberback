using Newtonsoft.Json;

namespace Uberback.Response
{
    public class Collect
    {
        [JsonProperty]
        public int Code;

        [JsonProperty]
        public Data[] Data;
    }
}
