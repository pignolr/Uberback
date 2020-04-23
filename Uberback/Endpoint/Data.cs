using Google.Cloud.Vision.V1;
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

        public class AnalyseBatchRequestDataBatch
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
            public List<AnalyseBatchRequestDataBatch> DataBatches { get; set; }
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
                foreach (var urlBatch in batch.DataBatches) {
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
                DataBatches = new List<AnalyseBatchRequestDataBatch>()
            };

            batch.Token = args.Get("token");
            batch.UserId = args.Get("userId");
            batch.Service = args.Get("service");

            int idxDataBatches = 0;
            string urlSrc;
            while (!string.IsNullOrEmpty((urlSrc = args.Get("dataBatches[" + idxDataBatches + "][urlSrc]"))))
            {
                batch.DataBatches.Add(new AnalyseBatchRequestDataBatch { UrlSrc = urlSrc, Images = new List<AnalyseBatchRequestData>(), Texts = new List<AnalyseBatchRequestData>() });

                int idxData = 0;
                while (!string.IsNullOrEmpty(args.Get("dataBatches[" + idxDataBatches + "][texts][" + idxData + "][nb]")))
                {
                    batch.DataBatches[idxDataBatches].Texts.Add(new AnalyseBatchRequestData
                    {
                        Content = args.Get("dataBatches[" + idxDataBatches + "][texts][" + idxData + "][content]"),
                        Nb = Int16.Parse(args.Get("dataBatches[" + idxDataBatches + "][texts][" + idxData + "][nb]"))
                    });
                    ++idxData;
                }

                idxData = 0;
                while (!string.IsNullOrEmpty(args.Get("dataBatches[" + idxDataBatches + "][images][" + idxData + "][nb]")))
                {
                    batch.DataBatches[idxDataBatches].Images.Add(new AnalyseBatchRequestData
                    {
                        Content = args.Get("dataBatches[" + idxDataBatches + "][images][" + idxData + "][content]"),
                        Nb = Int16.Parse(args.Get("dataBatches[" + idxDataBatches + "][images][" + idxData + "][nb]"))
                    });
                    ++idxData;
                }
                ++idxDataBatches;
            }
            return batch;
        }

        public static async Task<string> ConnectToAPIForAnalyseImageAsync(string userId, string urlSrc, string data, string service)
        {
            var imageUrl = Common.IsAbsoluteUrl(data) ? data : urlSrc + data;
            try
            {
                string flags;
                var urlWithSalt = imageUrl + Program.P.saltForHash;
                string hashedUrl = Common.GetHashString(urlWithSalt);
                // Check if the image is already analysed
                if (await Program.P.db.IsImageAnalysedAsync(hashedUrl))
                {
                    // Get old Flag
                    flags = await Program.P.db.GetFlagsFromAnalysedImageAsync(hashedUrl);
                    await Program.P.db.UpdateLastDateTimeOfAnalysedImageAsync(hashedUrl);
                } else
                {
                    // Analyse image
                    var trigeredFlags = await Program.P.ImageAnalyser.AnalyseImageUrlAsync(imageUrl);
                    flags = StringifyFlags(trigeredFlags);

                    // Store the result
                    await Program.P.db.AddAnalysedImageAsync(flags, hashedUrl);
                }
                // Log the analysed image
                Program.P.db.AddImageAsync(flags, userId, service).GetAwaiter().GetResult();
                return null;
            }
            catch (Exception e) when (e is System.Net.Http.HttpRequestException || e is ArgumentException || e is AnnotateImageException)
            {
                return "Error in the analyze of image \"" + imageUrl + "\": " + e.Message;
            }
        }

        public static async Task<string> ConnectToAPIForAnalyseTextAsync(string userId, string urlSrc, string text, string service)
        {
            try
            {
                string flags;
                var textWithSalt = text + Program.P.saltForHash;
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
            catch (Exception e) when (e is System.Net.Http.HttpRequestException)
            {
                return e.Message;
            }
        }

        private static string StringifyFlags(Dictionary<string, string> trigeredFlags)
            => string.Join(",", trigeredFlags.Keys);
    }
}
