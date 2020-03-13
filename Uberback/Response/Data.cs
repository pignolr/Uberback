using Newtonsoft.Json;

namespace Uberback.Response
{
    public enum DataType
    {
        Image,
        Text
    }

    public class Data
    {
        [JsonProperty]
        public string DateTime;

        [JsonProperty]
        public string Flags;

        [JsonProperty]
        public string UserId;

        [JsonProperty]
        public string Service;

        [JsonProperty]
        public DataType Type;
    }
}
