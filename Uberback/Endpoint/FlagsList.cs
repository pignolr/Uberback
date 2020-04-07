using Nancy;

namespace Uberback.Endpoint
{
    public class FlagsList : NancyModule
    {
        /// <summary>
        /// Get information
        /// </summary>
        public FlagsList() : base("/flagsList")
        {
            Get("/", x =>
            {
                return (Response.AsJson(new Response.FlagsList()
                {
                    FlagsImage = new[] { "SAFE", "Adult", "Medical", "Racy", "Violence" },
                    FlagsText = new[] { "SAFE", "SEVERE_TOXICITY", "IDENTITY_ATTACK", "INSULT", "PROFANITY", "THREAT", "INFLAMMATORY", "OBSCENE" }
                }));
            });
        }
    }
}
