using Newtonsoft.Json;

namespace Uberback.Response
{
    public class Error
    {
        [JsonProperty]
        public int Code;

        [JsonProperty]
        public string Message;
    }
}
