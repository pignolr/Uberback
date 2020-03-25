using Nancy;
using System.Collections.Specialized;

namespace Uberback.Validator
{
    class Analyse
    {
        public class ValidatorResponse
        {
            public string Message { get; set; }
            public HttpStatusCode StatusCode { get; set; }
        }

        public static ValidatorResponse ValidateRequest(NameValueCollection args)
        {
            ValidatorResponse error;
            if ((error = ValidateToken(args)) != null
                || (error = ValidateType(args)) != null
                || (error = ValidateUserId(args)) != null
                || (error = ValidateService(args)) != null
                || (error = ValidateUrlSrc(args)) != null
                || (error = ValidateData(args)) != null)
                return error;
            return null;
        }

        public static ValidatorResponse ValidateData(NameValueCollection args)
        {
            var data = args.Get("data");
            if (string.IsNullOrEmpty(data))
                return new ValidatorResponse { Message = "The data is not provided", StatusCode = HttpStatusCode.BadRequest };
            if (args.Get("type") == "image")
            {
                // Not a valid way to check if the url is an image
                if (!Endpoint.Common.IsAbsoluteUrl(data)
                    && !Endpoint.Common.IsRelativeUrl(data))
                    return new ValidatorResponse { Message = "The data is not an url of image", StatusCode = HttpStatusCode.BadRequest };
            }
            return null;
        }

        public static ValidatorResponse ValidateService(NameValueCollection args)
        {
            var urlSrc = args.Get("service");
            if (string.IsNullOrEmpty(urlSrc))
                return new ValidatorResponse { Message = "The service is not provided", StatusCode = HttpStatusCode.BadRequest };
            return null;
        }

        public static ValidatorResponse ValidateUrlSrc(NameValueCollection args)
        {
            var urlSrc = args.Get("urlSrc");
            if (string.IsNullOrEmpty(urlSrc))
                return new ValidatorResponse { Message = "The urlSrc is not provided", StatusCode = HttpStatusCode.BadRequest };
            if (!Endpoint.Common.IsAbsoluteUrl(urlSrc))
                return new ValidatorResponse { Message = "This urlSrc is not a valid Url", StatusCode = HttpStatusCode.BadRequest };
            return null;
        }

        public static ValidatorResponse ValidateUserId(NameValueCollection args)
        {
            var userId = args.Get("userId");
            if (string.IsNullOrEmpty(userId))
                return new ValidatorResponse { Message = "The userId is not provided", StatusCode = HttpStatusCode.BadRequest };
            if (!Endpoint.Common.IsUserExists(userId))
                return new ValidatorResponse { Message = "This userId not exist", StatusCode = HttpStatusCode.Forbidden };
            return null;
        }

        public static ValidatorResponse ValidateType(NameValueCollection args)
        {
            var type = args.Get("type");
            if (string.IsNullOrEmpty(type))
                return new ValidatorResponse { Message = "The type is not provided", StatusCode = HttpStatusCode.BadRequest };
            if (type != "image" && type != "text")
                return new ValidatorResponse { Message = "The type must be 'text' or 'image'", StatusCode = HttpStatusCode.BadRequest };
            return null;
        }

        public static ValidatorResponse ValidateToken(NameValueCollection args)
        {
            Endpoint.Common.Answer? error = Endpoint.Common.BasicCheck(args.Get("token"));
            if (error.HasValue)
                return new ValidatorResponse { Message = error.Value.message, StatusCode = HttpStatusCode.Forbidden };
            return null;
        }
    }
}
