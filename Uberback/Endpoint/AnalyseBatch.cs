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
            base.Post("/", async x =>
            {
                AnalyseBatchRequest args;
                string error = null;

                try
                {
                    args = Common.ParseJsonArgs<AnalyseBatchRequest>(Request.Body);

                    // Check request
                    if ((error = Validator.AnalyseBatch.ValidateRequest(args)) != null)
                        return Response.AsJson(new Response.Error() { Message = error }, HttpStatusCode.BadRequest);
                }
                catch (Exception e)
                {
                    return Response.AsJson(new Response.Error() { Message = e.Message }, HttpStatusCode.BadRequest);
                }
                //return Response.AsJson(new Response.Empty(), HttpStatusCode.NoContent);

                // Do request
                var taskErrors = new List<Task<string>>();
                foreach (var urlBatch in args.UrlBatchs) {
                    if (urlBatch.Images != null) {
                        foreach (var image in urlBatch.Images) {
                            taskErrors.Add(Analyze.ConnectToAPIForAnalyseImageAsync(args.UserId, urlBatch.UrlSrc, image.Data));
                        }
                    }
                    if (urlBatch.Texts != null) {
                        foreach (var text in urlBatch.Texts) {
                            taskErrors.Add(Analyze.ConnectToAPIForAnalyseTextAsync(args.UserId, urlBatch.UrlSrc, text.Data));
                        }
                    }
                }
                await Task.WhenAll(taskErrors);
                var errorsList = new List<string>();
                foreach (var response in taskErrors) {
                    if (response.Result != null) {
                        errorsList.Add(response.Result);
                    }
                }
                if (errorsList.Count != 0)
                    return Response.AsJson(new Response.ErrorArray() { Message = errorsList.ToArray() }, HttpStatusCode.BadRequest);
                else
                    return Response.AsJson(new Response.Empty(), HttpStatusCode.NoContent);
            });
        }
    }
}