using Nancy;
using RethinkDb.Driver.Net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
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
                string error;
                if ((error = Validator.Analyse.ValidateRequest(args)) != null)
                    return (Response.AsJson(new Response.Error() { Message = error }, HttpStatusCode.BadRequest));

                var userId = args.Get("userId");
                var urlSrc = args.Get("urlSrc");
                var type = args.Get("type");
                var data = args.Get("data");

                if (type == "image")
                    return await AnalyseImageAsync(userId, urlSrc, data);
                else if (type == "text")
                    return await AnalyseTextAsync(userId, urlSrc, data);
                return Response.AsJson(new Response.Empty(), HttpStatusCode.NoContent);
            });
        }

        private async Task<Nancy.Response> AnalyseImageAsync(string userId, string urlSrc, string data)
        {
            try
            {
                var imageUrl = Common.IsAbsoluteUrl(data) ? data : urlSrc + data;
                var trigeredFlags = await Program.P.ImageAnalyser.AnalyseImageUrlAsync(imageUrl);
                var flags = StringifyFlags(trigeredFlags);

                Program.P.db.AddImageAsync(flags, userId).GetAwaiter().GetResult();
                return Response.AsJson(new Response.Empty(), HttpStatusCode.NoContent);
            }
            catch (Exception e)
            {
                return Response.AsJson(
                    new Response.Error() { Message = e.Message },
                    HttpStatusCode.InternalServerError);
            }
        }

        private async Task<Nancy.Response> AnalyseTextAsync(string userId, string urlSrc, string data)
        {
            try
            {
                var trigeredFlags = await Program.P.TextAnalyser.AnalyseTextAsync(data);
                var flags = StringifyFlags(trigeredFlags);
            
                Program.P.db.AddTextAsync(flags, userId).GetAwaiter().GetResult();
                return Response.AsJson(new Response.Empty(), HttpStatusCode.NoContent);
            }
            catch (Exception e)
            {
                return Response.AsJson(
                    new Response.Error() { Message = e.Message },
                    HttpStatusCode.InternalServerError);
            }
        }

        private static string StringifyFlags(Dictionary<string, string> trigeredFlags)
            => string.Join(",", trigeredFlags.Keys);
    }

}
