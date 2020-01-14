using Newtonsoft.Json;

namespace Uberback.Response
{
    public class ErrorArray
    {
        [JsonProperty]
        public string[] Message;
    }
}
