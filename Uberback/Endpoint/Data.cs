using Nancy;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Uberback.Endpoint
{
    public class Data : NancyModule
    {
        public class AnalyseBatchRequestData
        {
            public string Content { get; set; }
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

        public Data() : base("/data")
        {
            base.Post("/", async x =>
            {
                var args = Common.ParseArgs(Request.Body);

                AnalyseBatchRequest batch;
                try
                {
                    batch = ParseBatchArgs(args);

                    // Check request
                    Validator.Data.ValidatorResponse error = null;
                    if ((error = Validator.Data.ValidateRequest(batch)) != null)
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
                            taskErrors.Add(ConnectToAPIForAnalyseImageAsync(batch.UserId, urlBatch.UrlSrc, image.Content, batch.Service));
                        }
                    }
                    if (urlBatch.Texts != null) {
                        foreach (var text in urlBatch.Texts) {
                            taskErrors.Add(ConnectToAPIForAnalyseTextAsync(batch.UserId, urlBatch.UrlSrc, text.Content, batch.Service));
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
                        Content = args.Get("urlBatchs[" + idxUrlBatch + "][texts][" + idxData + "][content]"),
                        Nb = Int16.Parse(args.Get("urlBatchs[" + idxUrlBatch + "][texts][" + idxData + "][nb]"))
                    });
                    ++idxData;
                }

                idxData = 0;
                while (!string.IsNullOrEmpty(args.Get("urlBatchs[" + idxUrlBatch + "][images][" + idxData + "][nb]")))
                {
                    batch.UrlBatchs[idxUrlBatch].Images.Add(new AnalyseBatchRequestData
                    {
                        Content = args.Get("urlBatchs[" + idxUrlBatch + "][images][" + idxData + "][content]"),
                        Nb = Int16.Parse(args.Get("urlBatchs[" + idxUrlBatch + "][images][" + idxData + "][nb]"))
                    });
                    ++idxData;
                }
                ++idxUrlBatch;
            }
            return batch;
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

        private static string StringifyFlags(Dictionary<string, string> trigeredFlags)
            => string.Join(",", trigeredFlags.Keys);
    }
}