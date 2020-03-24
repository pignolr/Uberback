using Newtonsoft.Json;
using System.Collections.Generic;

namespace Uberback.Response
{
    public class Collect
    {
        [JsonProperty]
        public Dictionary<string, FlagData[]> Datas;
    }
}
