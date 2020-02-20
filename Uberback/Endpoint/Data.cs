using Google.Cloud.Vision.V1;
using Nancy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Uberback.Endpoint
{
    public class Data : NancyModule
    {
        /// <summary>
        /// Post informationx
        /// </summary>
        public Data() : base("/data")
        {
            Post("/", x =>
            {
                var args = Common.ParseArgs(Request.Body);

                // Error Handling
                Common.Answer? error = Common.BasicCheck(args.Get("token"));
                if (error.HasValue)
                    return (Response.AsJson(new Response.Error()
                    {
                        Message = error.Value.message
                    }, error.Value.code));
                if (string.IsNullOrEmpty(args.Get("type")) || string.IsNullOrEmpty(args.Get("userId")) || string.IsNullOrEmpty(args.Get("content")) || string.IsNullOrEmpty(args.Get("token")))
                    return (Response.AsJson(new Response.Error()
                    {
                        Message = "Missing arguments"
                    }, HttpStatusCode.BadRequest));

                List<string> flags = new List<string>();
                // Add information to the db
                switch (args.Get("type").ToString())
                {
                    case "text":
                        string finalMsg = Program.P.translationClient.TranslateText(args.Get("content"), "en").TranslatedText;
                        using (HttpClient hc = new HttpClient())
                        {
                            HttpResponseMessage post = hc.PostAsync("https://commentanalyzer.googleapis.com/v1alpha1/comments:analyze?key=" + Program.P.perspectiveApi, new StringContent(
                                    JsonConvert.DeserializeObject("{comment: {text: \"" + EscapeString(finalMsg) + "\"},"
                                                                + "languages: [\"en\"],"
                                                                + "requestedAttributes: {" + string.Join(":{}, ", categories.Select(y => y.Item1)) + ":{}} }").ToString(), Encoding.UTF8, "application/json")).GetAwaiter().GetResult();

                            dynamic json = JsonConvert.DeserializeObject(post.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                            foreach (var s in categories)
                            {
                                double value = json.attributeScores[s.Item1].summaryScore.value;
                                if (value >= s.Item2)
                                    flags.Add(s.Item1);
                            }
                            if (flags.Count == 0)
                                flags.Add("SAFE");
                        }
                        Program.P.db.AddTextAsync(string.Join(",", flags), args.Get("userId")).GetAwaiter().GetResult();
                        break;

                    case "image":
                        var image = Image.FetchFromUri(args.Get("content"));
                        SafeSearchAnnotation response = Program.P.imageClient.DetectSafeSearch(image);
                        if (response.Adult > Likelihood.Possible)
                            flags.Add("Adult");
                        if (response.Medical > Likelihood.Possible)
                            flags.Add("Medical");
                        if (response.Racy > Likelihood.Possible)
                            flags.Add("Racy");
                        if (response.Violence > Likelihood.Possible)
                            flags.Add("Violence");
                        if (flags.Count == 0)
                            flags.Add("SAFE");
                        Program.P.db.AddImageAsync(string.Join(",", flags), args.Get("userId")).GetAwaiter().GetResult();
                        break;

                    default:
                        return (Response.AsJson(new Response.Error()
                        {
                            Message = "Type must be text or image"
                        }, HttpStatusCode.BadRequest));
                }
                return (Response.AsJson(new Response.Empty(), HttpStatusCode.NoContent));
            });
        }

        private static string EscapeString(string msg)
        {
            return msg.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private readonly Tuple<string, float>[] categories = new Tuple<string, float>[] {
            new Tuple<string, float>("TOXICITY", .80f),
            new Tuple<string, float>("SEVERE_TOXICITY", .60f),
            new Tuple<string, float>("IDENTITY_ATTACK", .60f),
            new Tuple<string, float>("INSULT", .60f),
            new Tuple<string, float>("PROFANITY", .80f),
            new Tuple<string, float>("THREAT", .60f),
            new Tuple<string, float>("INFLAMMATORY", .60f),
            new Tuple<string, float>("OBSCENE", .9f)
        };
    }
}
