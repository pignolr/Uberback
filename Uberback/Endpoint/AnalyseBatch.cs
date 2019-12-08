using Nancy;
using Newtonsoft.Json;
using RethinkDb.Driver.Net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Uberback.Endpoint
{
    public class AnalyseBatchRequestData
    {
        public string data { get; set; }
        public string urlSrc { get; set; }
        public int nb { get; set; }
    }

    public class AnalyseBatchRequest
    {
        public AnalyseBatchRequestData[] texts { get; set; }
        public AnalyseBatchRequestData[] images { get; set; }
        public string userId { get; set; }
        public string token { get; set; }
    }

    public class AnalyzeBatch : NancyModule
    {
        public AnalyzeBatch() : base("/analyzeBatch")
        {
            base.Post("/", x =>
            {
                AnalyseBatchRequest args;
                
                try
                {
                    string error;
                    args = Common.ParseJsonArgs<AnalyseBatchRequest>(Request.Body);

                    // Check request
                    if ((error = Validator.AnalyseBatch.ValidateRequest(args)) != null)
                        return (Response.AsJson(new Response.Error() { Message = error }, HttpStatusCode.BadRequest));
                }
                catch (Exception e)
                {
                    return (Response.AsJson(new Response.Error() { Message = e.Message }, HttpStatusCode.BadRequest));
                }
                // Do request
                return Response.AsJson(new Response.Empty(), HttpStatusCode.NoContent);
            });
        }
    }
}