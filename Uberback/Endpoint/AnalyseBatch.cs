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
        public string Data { get; set; }
        public int Nb { get; set; }
    }

    public class AnalyseBatchRequestUrlBatch
    {
        public string UrlSrc { get; set; }
        public AnalyseBatchRequestData[] Images { get; set; }
        public AnalyseBatchRequestData[] Texts { get; set; }
    }

    public class AnalyseBatchRequest
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        public AnalyseBatchRequestUrlBatch[] UrlBatchs { get; set; }
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