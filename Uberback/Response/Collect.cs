using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Uberback.Response
{
    public class Collect
    {
        [JsonProperty]
        public int Code;

        [JsonProperty]
        public List<JObject> Data;
    }
}
