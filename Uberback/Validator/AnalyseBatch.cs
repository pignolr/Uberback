using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Uberback.Validator
{
    class AnalyseBatch
    {
        public static string ValidateRequest(Endpoint.AnalyseBatchRequest args)
        {
            string error;
            if ((error = ValidateBatchRequest(args)) != null
                ||(error = ValidateToken(args)) != null
                || (error = ValidateUserId(args)) != null
                || (error = ValidateUrlBatch(args)) != null)
                return error;
            return null;
        }
        public static string ValidateBatchRequest(Endpoint.AnalyseBatchRequest args)
        {
            if (args == null)
                return "Invalid format of request";
            return null;
        }
            public static string ValidateUrlBatch(Endpoint.AnalyseBatchRequest args)
        {
            if (args.UrlBatchs == null
                || args.UrlBatchs.Length == 0)
                return "No Batch provided";
            string error;
            foreach (var urlBatch in args.UrlBatchs)
            {
                if (!Endpoint.Common.IsAbsoluteUrl(urlBatch.UrlSrc)
                    && !Endpoint.Common.IsRelativeUrl(urlBatch.UrlSrc))
                    return "Invalid item in the list of batch of url: \"urlSrc\" must be an url";
                if ((error = ValidateImages(urlBatch, urlBatch.UrlSrc)) != null
                    || (error = ValidateTexts(urlBatch, urlBatch.UrlSrc)) != null)
                    return error;
            }
            return null;
        }

        public static string ValidateImages(Endpoint.AnalyseBatchRequestUrlBatch args, string urlSrc)
        {
            if (args.Images == null)
                return null;
            foreach (var item in args.Images)
            {
                if (item.Nb == 0 || string.IsNullOrEmpty(item.Data))
                    return "Invalid item in the list of images of \"" + urlSrc + "\": doesn't contain \"data\" or \"nb\"";
                // Not a valid way to check if the url is an image
/*                    if (!Endpoint.Common.IsAbsoluteUrl(item.data)
                    && !Endpoint.Common.IsRelativeUrl(item.data))
                    return "Invalid item in the list of images: \"data\" must be an url";*/
            }
            return null;
        }

        public static string ValidateTexts(Endpoint.AnalyseBatchRequestUrlBatch args, string urlSrc)
        {
            if (args.Texts ==  null)
                return null;
            foreach (var item in args.Texts)
            {
                if (item.Nb == 0 || string.IsNullOrEmpty(item.Data))
                    return "Invalid item in the list of texts of \"" + urlSrc + "\": doesn't contain \"data\" or \"nb\"";
            }
            return null;
        }

        public static string ValidateToken(Endpoint.AnalyseBatchRequest args)
        {
            Endpoint.Common.Answer? error = Endpoint.Common.BasicCheck(args.Token);
            if (error.HasValue)
                return error.Value.message;
            return null;
        }

        public static string ValidateUserId(Endpoint.AnalyseBatchRequest args)
        {
            if (string.IsNullOrEmpty(args.UserId))
                return "The userId is not provided";
            if (!Endpoint.Common.IsUserExists(args.UserId))
                return "This userId not exist";
            return null;
        }

    }
}
