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
    public class AnalyzeBatch : NancyModule
    {
        public class AnalyseBatchRequestData
        {
            public string Data { get; set; }
            public int Nb { get; set; }
        }

        public class AnalyseBatchRequestUrlBatch
        {
            public string UrlSrc { get; set; }
            public List<AnalyseBatchRequestData> Images { get; set; }
            public List<AnalyseBatchRequestData> Texts { get; set; }
        }

        public class AnalyseBatchRequest
        {
            public string Token { get; set; }
            public string UserId { get; set; }
            public string Service { get; set; }
            public List<AnalyseBatchRequestUrlBatch> UrlBatchs { get; set; }
        }

        public AnalyzeBatch() : base("/analyzeBatch")
        {
            base.Post("/", async x =>
            {
                var args = Common.ParseArgs(Request.Body);

                AnalyseBatchRequest batch;
                try
                {
                    batch = ParseBatchArgs(args);

                    // Check request
                    Validator.AnalyseBatch.ValidatorResponse error = null;
                    if ((error = Validator.AnalyseBatch.ValidateRequest(batch)) != null)
                        return Response.AsJson(new Response.Error() { Message = error.Message }, error.StatusCode);
                }
                catch (Exception e)
                {
                    return Response.AsJson(new Response.Error() { Message = e.Message }, HttpStatusCode.BadRequest);
                }

                // Do request
                var taskErrors = new List<Task<string>>();
                foreach (var urlBatch in batch.UrlBatchs) {
                    if (urlBatch.Images != null) {
                        foreach (var image in urlBatch.Images) {
                            taskErrors.Add(Analyze.ConnectToAPIForAnalyseImageAsync(batch.UserId, urlBatch.UrlSrc, image.Data, batch.Service));
                        }
                    }
                    if (urlBatch.Texts != null) {
                        foreach (var text in urlBatch.Texts) {
                            taskErrors.Add(Analyze.ConnectToAPIForAnalyseTextAsync(batch.UserId, urlBatch.UrlSrc, text.Data, batch.Service));
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

        public AnalyseBatchRequest ParseBatchArgs(NameValueCollection args)
        {
            var batch = new AnalyseBatchRequest
            {
                UrlBatchs = new List<AnalyseBatchRequestUrlBatch>()
            };

            batch.Token = args.Get("token");
            batch.UserId = args.Get("userId");
            batch.Service = args.Get("service");

            int idxUrlBatch = 0;
            string urlSrc;
            while (!string.IsNullOrEmpty((urlSrc = args.Get("urlBatchs[" + idxUrlBatch + "][urlSrc]"))))
            {
                batch.UrlBatchs.Add(new AnalyseBatchRequestUrlBatch { UrlSrc = urlSrc, Images = new List<AnalyseBatchRequestData>(), Texts = new List<AnalyseBatchRequestData>() });

                int idxData = 0;
                while (!string.IsNullOrEmpty(args.Get("urlBatchs[" + idxUrlBatch + "][texts][" + idxData + "][nb]")))
                {
                    batch.UrlBatchs[idxUrlBatch].Texts.Add(new AnalyseBatchRequestData
                    {
                        Data = args.Get("urlBatchs[" + idxUrlBatch + "][texts][" + idxData + "][data]"),
                        Nb = Int16.Parse(args.Get("urlBatchs[" + idxUrlBatch + "][texts][" + idxData + "][nb]"))
                    });
                    ++idxData;
                }

                idxData = 0;
                while (!string.IsNullOrEmpty(args.Get("urlBatchs[" + idxUrlBatch + "][images][" + idxData + "][nb]")))
                {
                    batch.UrlBatchs[idxUrlBatch].Images.Add(new AnalyseBatchRequestData
                    {
                        Data = args.Get("urlBatchs[" + idxUrlBatch + "][images][" + idxData + "][data]"),
                        Nb = Int16.Parse(args.Get("urlBatchs[" + idxUrlBatch + "][images][" + idxData + "][nb]"))
                    });
                    ++idxData;
                }
                ++idxUrlBatch;
            }
            return batch;
        }
    }
}