using System.Collections.Specialized;

namespace Uberback.Validator
{
    class Analyse
    {
        public static string ValidateRequest(NameValueCollection args)
        {
            string error;
            if ((error = ValidateToken(args)) != null
                || (error = ValidateType(args)) != null
                || (error = ValidateUserId(args)) != null
                || (error = ValidateUrlSrc(args)) != null
                || (error = ValidateData(args)) != null)
                return error;
            return null;
        }


        public static string ValidateData(NameValueCollection args)
        {
            var data = args.Get("data");
            if (string.IsNullOrEmpty(data))
                return "The data is not provided";
            if (args.Get("type") == "image")
            {
                // Not a valid way to check if the url is an image
                if (!Endpoint.Common.IsAbsoluteUrl(data)
                    && !Endpoint.Common.IsRelativeUrl(data))
                    return "The data is not an url of image";
            }
            return null;
        }

        public static string ValidateUrlSrc(NameValueCollection args)
        {
            var urlSrc = args.Get("urlSrc");
            if (string.IsNullOrEmpty(urlSrc))
                return "The urlSrc is not provided";
            if (!Endpoint.Common.IsAbsoluteUrl(urlSrc))
                return "This urlSrc is not a valid Url";
            return null;
        }

        public static string ValidateUserId(NameValueCollection args)
        {
            var userId = args.Get("userId");
            if (string.IsNullOrEmpty(userId))
                return "The userId is not provided";
            if (!Endpoint.Common.IsUserExists(userId))
                return "This userId not exist";
            return null;
        }

        public static string ValidateType(NameValueCollection args)
        {
            var type = args.Get("type");
            if (string.IsNullOrEmpty(type))
                return "The type is not provided";
            if (type != "image" && type != "text")
                return "The type must be 'text' or 'image'";
            return null;
        }

        public static string ValidateToken(NameValueCollection args)
        {
            Endpoint.Common.Answer? error = Endpoint.Common.BasicCheck(args.Get("token"));
            if (error.HasValue)
                return error.Value.message;
            return null;
        }
    }
}
