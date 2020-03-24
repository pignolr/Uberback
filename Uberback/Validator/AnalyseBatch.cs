using Newtonsoft.Json;
using Nancy;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Uberback.Validator
{
    class AnalyseBatch
    {
        public class ValidatorResponse
        {
            public string Message { get; set; }
            public HttpStatusCode StatusCode { get; set; }
        }

        public static ValidatorResponse ValidateRequest(Endpoint.AnalyseBatchRequest args)
        {
            ValidatorResponse error;
            if ((error = ValidateBatchRequest(args)) != null
                ||(error = ValidateToken(args)) != null
                || (error = ValidateUserId(args)) != null
                || (error = ValidateService(args)) != null
                || (error = ValidateUrlBatch(args)) != null)
                return error;
            return null;
        }
        public static ValidatorResponse ValidateBatchRequest(Endpoint.AnalyseBatchRequest args)
        {
            if (args == null)
                return new ValidatorResponse { Message = "Invalid format of request", StatusCode = HttpStatusCode.BadRequest };
            return null;
        }
            public static ValidatorResponse ValidateUrlBatch(Endpoint.AnalyseBatchRequest args)
        {
            if (args.UrlBatchs == null
                || args.UrlBatchs.Length == 0)
                return new ValidatorResponse { Message = "No Batch provided", StatusCode = HttpStatusCode.BadRequest };
            ValidatorResponse error;
            foreach (var urlBatch in args.UrlBatchs)
            {
                if (!Endpoint.Common.IsAbsoluteUrl(urlBatch.UrlSrc)
                    && !Endpoint.Common.IsRelativeUrl(urlBatch.UrlSrc))
                    return new ValidatorResponse { Message = "Invalid item in the list of batch of url: \"urlSrc\" must be an url", StatusCode = HttpStatusCode.BadRequest };
                if ((error = ValidateImages(urlBatch, urlBatch.UrlSrc)) != null
                    || (error = ValidateTexts(urlBatch, urlBatch.UrlSrc)) != null)
                    return error;
            }
            return null;
        }

        public static ValidatorResponse ValidateImages(Endpoint.AnalyseBatchRequestUrlBatch args, string urlSrc)
        {
            if (args.Images == null)
                return null;
            foreach (var item in args.Images)
            {
                if (item.Nb == 0 || string.IsNullOrEmpty(item.Data))
                    return new ValidatorResponse { Message = "Invalid item in the list of images of \"" + urlSrc + "\": doesn't contain \"data\" or \"nb\"", StatusCode = HttpStatusCode.BadRequest };
                // Not a valid way to check if the url is an image
/*                    if (!Endpoint.Common.IsAbsoluteUrl(item.data)
                    && !Endpoint.Common.IsRelativeUrl(item.data))
                    return new ValidatorResponse { Message = "Invalid item in the list of images: \"data\" must be an url", StatusCode = HttpStatusCode.BadRequest };*/
            }
            return null;
        }

        public static ValidatorResponse ValidateTexts(Endpoint.AnalyseBatchRequestUrlBatch args, string urlSrc)
        {
            if (args.Texts ==  null)
                return null;
            foreach (var item in args.Texts)
            {
                if (item.Nb == 0 || string.IsNullOrEmpty(item.Data))
                    return new ValidatorResponse { Message = "Invalid item in the list of texts of \"" + urlSrc + "\": doesn't contain \"data\" or \"nb\"", StatusCode = HttpStatusCode.BadRequest };
            }
            return null;
        }

        public static ValidatorResponse ValidateToken(Endpoint.AnalyseBatchRequest args)
        {
            Endpoint.Common.Answer? error = Endpoint.Common.BasicCheck(args.Token);
            if (error.HasValue)
                return new ValidatorResponse { Message = error.Value.message, StatusCode = HttpStatusCode.Forbidden };
            return null;
        }

        public static ValidatorResponse ValidateUserId(Endpoint.AnalyseBatchRequest args)
        {
            if (string.IsNullOrEmpty(args.UserId))
                return new ValidatorResponse { Message = "The userId is not provided", StatusCode = HttpStatusCode.BadRequest };
            if (!Endpoint.Common.IsUserExists(args.UserId))
                return new ValidatorResponse { Message = "This userId not exist", StatusCode = HttpStatusCode.Forbidden };
            return null;
        }

        public static ValidatorResponse ValidateService(Endpoint.AnalyseBatchRequest args)
        {
            if (string.IsNullOrEmpty(args.Service))
                return new ValidatorResponse { Message = "The Service is not provided", StatusCode = HttpStatusCode.BadRequest };
            return null;
        }
    }
}
