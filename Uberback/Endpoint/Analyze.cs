using Nancy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Uberback.Endpoint
{
    public class Analyze : NancyModule
    {
        public Analyze() : base("/analyze")
        {
            base.Post("/", async x =>
            {
                var args = Common.ParseArgs(Request.Body);

                // Check request
                Validator.Analyse.ValidatorResponse error;
                if ((error = Validator.Analyse.ValidateRequest(args)) != null)
                    return Response.AsJson(new Response.Error() { Message = error.Message }, error.StatusCode);

                var userId = args.Get("userId");
                var urlSrc = args.Get("urlSrc");
                var type = args.Get("type");
                var data = args.Get("data");
                var service = args.Get("service");

                if (type == "image")
                    return await AnalyseImageAsync(userId, urlSrc, data, service);
                else if (type == "text")
                    return await AnalyseTextAsync(userId, urlSrc, data, service);
                return Response.AsJson(new Response.Empty(), HttpStatusCode.NoContent);
            });
        }

        public static async Task<string> ConnectToAPIForAnalyseImageAsync(string userId, string urlSrc, string data, string service)
        {
            var imageUrl = Common.IsAbsoluteUrl(data) ? data : urlSrc + data;
            try
            {
                var trigeredFlags = await Program.P.ImageAnalyser.AnalyseImageUrlAsync(imageUrl);
                var flags = StringifyFlags(trigeredFlags);

                Program.P.db.AddImageAsync(flags, userId, service).GetAwaiter().GetResult();
                return null;
            }
            catch (Exception e)
            {
                return "Error in the analyze of image \"" + imageUrl + "\": " + e.Message;
            }
        }

        public static async Task<string> ConnectToAPIForAnalyseTextAsync(string userId, string urlSrc, string text, string service)
        {
            try
            {
                string flags;
                var textWithSalt = text + "0nes@l7yb0y¨^";
                string hashedText = Common.GetHashString(textWithSalt);
                // Check if the text is already analysed
                if (await Program.P.db.IsTextAnalysedAsync(hashedText))
                {
                    // Get old Flag
                    flags = await Program.P.db.GetFlagsFromAnalysedTextAsync(hashedText);
                    await Program.P.db.UpdateLastDateTimeOfAnalysedTextAsync(hashedText);
                }
                else
                {
                    // Analyse text
                    var trigeredFlags = await Program.P.TextAnalyser.AnalyseTextAsync(text);
                    flags = StringifyFlags(trigeredFlags);

                    // Store the result
                    await Program.P.db.AddAnalysedTextAsync(flags, hashedText);
                }
                // Log the analysed text
                await Program.P.db.AddTextAsync(flags, userId, service);
                return null;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private async Task<Nancy.Response> AnalyseImageAsync(string userId, string urlSrc, string data, string service)
        {
            string error;
            if ((error = await ConnectToAPIForAnalyseImageAsync(userId, urlSrc, data, service)) != null)
                return Response.AsJson(new Response.Error() { Message = error }, HttpStatusCode.InternalServerError);
            else
                return Response.AsJson(new Response.Empty(), HttpStatusCode.NoContent);
        }

        private async Task<Nancy.Response> AnalyseTextAsync(string userId, string urlSrc, string data, string service)
        {
            string error;
            if ((error = await ConnectToAPIForAnalyseTextAsync(userId, urlSrc, data, service)) != null)
                return Response.AsJson(new Response.Error() { Message = error }, HttpStatusCode.InternalServerError);
            else
                return Response.AsJson(new Response.Empty(), HttpStatusCode.NoContent);
        }

        private static string StringifyFlags(Dictionary<string, string> trigeredFlags)
            => string.Join(",", trigeredFlags.Keys);
    }
}
