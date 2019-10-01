using Nancy;
using Nancy.Helpers;
using System;
using System.Collections.Specialized;
using System.IO;

namespace Uberback.Endpoint
{
    public static class Common
    {
        /// <summary>
        /// Basic check when contacting an endpoint
        /// Check things like spam prevention or invalid token when doing requests
        /// </summary>
        /// <param name="token">User token</param>
        /// <returns>Error, null is request is valid</returns>
        public static Answer? BasicCheck(string token)
        {
            if (string.IsNullOrEmpty(token))
                return new Answer(HttpStatusCode.BadRequest, "Missing arguments");
            if (token != Program.P.token)
                return new Answer(HttpStatusCode.Unauthorized, "Bad token");
            return null;
        }

        public static NameValueCollection ParseArgs(Stream bodyStream)
        {
            string body;
            using (var reader = new StreamReader(bodyStream)) // x-www-form-urlencoded
                body = reader.ReadToEnd();
            return HttpUtility.ParseQueryString(body);
        }

        public struct Answer
        {
            public Answer(HttpStatusCode mcode, string mmessage)
            {
                code = mcode;
                message = mmessage;
            }
            public HttpStatusCode code;
            public string message;
        }

        public static bool IsAbsoluteUrl(string url)
        => Uri.IsWellFormedUriString(url, UriKind.Absolute);

        public static bool IsRelativeUrl(string url)
        => Uri.IsWellFormedUriString(url, UriKind.Relative);

        public static bool IsUrl(string url)
        => Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);

        public static bool IsUserExists(string UserExist)
        => true;

    }
}
