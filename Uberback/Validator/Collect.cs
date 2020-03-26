using Nancy;
using System;
using System.Collections.Specialized;
using System.Globalization;

namespace Uberback.Validator
{
    class Collect
    {
        public class ValidatorResponse
        {
            public string Message { get; set; }
            public HttpStatusCode StatusCode { get; set; }
        }

        public static ValidatorResponse ValidateRequest(NameValueCollection args)
        {
            ValidatorResponse error;
            if ((error = ValidateToken(args.Get("token"))) != null
                || (error = ValidateType(args.Get("type"))) != null
                || (error = ValidateFrom(args.Get("from"))) != null
                || (error = ValidateTo(args.Get("to"))) != null)
                return error;
            return null;
        }

        public static ValidatorResponse ValidateToken(string token)
        {
            Endpoint.Common.Answer? error = Endpoint.Common.BasicCheck(token);
            if (error.HasValue)
                return new ValidatorResponse { Message = error.Value.message, StatusCode = error.Value.code };
            return null;
        }

        public static ValidatorResponse ValidateType(string type)
        {
            if (!string.IsNullOrEmpty(type) && type != "image" && type != "text")
                return new ValidatorResponse { Message = "If set, type must be text or image", StatusCode = HttpStatusCode.BadRequest };
            return null;
        }

        public static ValidatorResponse ValidateFrom(string from)
        {
            if (!string.IsNullOrEmpty(from) && !DateTime.TryParseExact(from, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dummy))
                return new ValidatorResponse { Message = "From must be in the format yyyyMMdd", StatusCode = HttpStatusCode.BadRequest };
            return null;
        }

        public static ValidatorResponse ValidateTo(string to)
        {
            if (!string.IsNullOrEmpty(to) && !DateTime.TryParseExact(to, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dummy))
                return new ValidatorResponse { Message = "To must be in the format yyyyMMdd", StatusCode = HttpStatusCode.BadRequest };
            return null;
        }
    }
}
