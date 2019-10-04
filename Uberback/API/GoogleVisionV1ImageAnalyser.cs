using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Vision.V1;

namespace Uberback.API
{
    class GoogleVisionV1ImageAnalyser: IImageAnalyser
    {
        private readonly ImageAnnotatorClient ImageClient;
        public GoogleVisionV1ImageAnalyser(string imageAPIKeysFile)
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", imageAPIKeysFile);
            ImageClient = ImageAnnotatorClient.Create();
        }

        public async Task<Dictionary<string, string>> AnalyseImageUrlAsync(string url)
        {
            if (url.Length == 0)
                return null;
            var image = await Image.FetchFromUriAsync(url);
            var response = await ImageClient.DetectSafeSearchAsync(image);
            return GetTrigeredFlags(response);
        }

        private Dictionary<string, string> GetTrigeredFlags(SafeSearchAnnotation response)
        {
            var flags = new Dictionary<string, string>();

            if (response.Adult > Likelihood.Possible)
                flags["Adult"] = response.Adult.ToString();
            if (response.Medical > Likelihood.Possible)
                flags["Medical"] = response.Medical.ToString();
            if (response.Racy > Likelihood.Possible)
                flags["Racy"] = response.Racy.ToString();
            if (response.Violence > Likelihood.Possible)
                flags["Violence"] = response.Violence.ToString();
            if (flags.Count == 0)
                flags["SAFE"] = "1.00";
            return flags;
        }
    }
}
