﻿using Nancy;

namespace Uberback.Validator
{
    class Data
    {
        public class ValidatorResponse
        {
            public string Message { get; set; }
            public HttpStatusCode StatusCode { get; set; }
        }

        public static ValidatorResponse ValidateRequest(Endpoint.Data.AnalyseBatchRequest args)
        {
            ValidatorResponse error;
            if ((error = ValidateBatchRequest(args)) != null
                || (error = ValidateToken(args)) != null
                || (error = ValidateUserId(args)) != null
                || (error = ValidateService(args)) != null
                || (error = ValidateUrlBatch(args)) != null)
                return error;
            return null;
        }

        public static ValidatorResponse ValidateBatchRequest(Endpoint.Data.AnalyseBatchRequest args)
        {
            if (args == null)
                return new ValidatorResponse { Message = "Invalid format of request", StatusCode = HttpStatusCode.BadRequest };
            return null;
        }

        public static ValidatorResponse ValidateUrlBatch(Endpoint.Data.AnalyseBatchRequest args)
        {
            if (args.DataBatches == null || args.DataBatches.Count == 0)
                return new ValidatorResponse { Message = "No Batch provided", StatusCode = HttpStatusCode.BadRequest };
            ValidatorResponse error;
            foreach (var dataBatch in args.DataBatches)
            {
                if (!Endpoint.Common.IsAbsoluteUrl(dataBatch.UrlSrc)
                    && !Endpoint.Common.IsRelativeUrl(dataBatch.UrlSrc))
                    return new ValidatorResponse { Message = "Invalid item in the list of batch of url: \"urlSrc\" must be an url", StatusCode = HttpStatusCode.BadRequest };
                if ((error = ValidateImages(dataBatch, dataBatch.UrlSrc)) != null
                    || (error = ValidateTexts(dataBatch, dataBatch.UrlSrc)) != null)
                    return error;
            }
            return null;
        }

        public static ValidatorResponse ValidateImages(Endpoint.Data.AnalyseBatchRequestDataBatch args, string urlSrc)
        {
            if (args.Images == null)
                return null;
            foreach (var item in args.Images)
            {
                if (item.Nb == 0 || string.IsNullOrEmpty(item.Content))
                    return new ValidatorResponse { Message = "Invalid item in the list of images of \"" + urlSrc + "\": doesn't contain \"data\" or \"nb\"", StatusCode = HttpStatusCode.BadRequest };
            }
            return null;
        }

        public static ValidatorResponse ValidateTexts(Endpoint.Data.AnalyseBatchRequestDataBatch databatch, string urlSrc)
        {
            if (databatch.Texts ==  null)
                return null;
            foreach (var item in databatch.Texts)
            {
                if (item.Nb == 0 || string.IsNullOrEmpty(item.Content))
                    return new ValidatorResponse { Message = "Invalid item in the list of texts of \"" + urlSrc + "\": doesn't contain \"data\" or \"nb\"", StatusCode = HttpStatusCode.BadRequest };
            }
            return null;
        }

        public static ValidatorResponse ValidateToken(Endpoint.Data.AnalyseBatchRequest args)
        {
            Endpoint.Common.Answer? error = Endpoint.Common.BasicCheck(args.Token);
            if (error.HasValue)
                return new ValidatorResponse { Message = error.Value.message, StatusCode = error.Value.code };
            return null;
        }

        public static ValidatorResponse ValidateUserId(Endpoint.Data.AnalyseBatchRequest args)
        {
            if (string.IsNullOrEmpty(args.UserId))
                return new ValidatorResponse { Message = "The userId is not provided", StatusCode = HttpStatusCode.BadRequest };
            if (!Endpoint.Common.IsUserExists(args.UserId))
                return new ValidatorResponse { Message = "This userId not exist", StatusCode = HttpStatusCode.BadRequest };
            return null;
        }

        public static ValidatorResponse ValidateService(Endpoint.Data.AnalyseBatchRequest args)
        {
            if (string.IsNullOrEmpty(args.Service))
                return new ValidatorResponse { Message = "The Service is not provided", StatusCode = HttpStatusCode.BadRequest };
            return null;
        }
    }
}
