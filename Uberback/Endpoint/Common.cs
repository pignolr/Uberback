using Nancy;
using Nancy.Helpers;
using System;
using System.Collections.Specialized;
using System.IO;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

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

        public static JsonObjectFormat ParseJsonArgs<JsonObjectFormat>(Stream bodyStream)
        {
            string body;
            using (var reader = new StreamReader(bodyStream)) // x-www-form-urlencoded
                body = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<JsonObjectFormat>(body);
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

        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}
