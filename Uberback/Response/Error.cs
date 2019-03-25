using Newtonsoft.Json;

namespace Uberback.Response
{
    public class Error
    {
        [JsonProperty]
        public string Message;
    }
}
