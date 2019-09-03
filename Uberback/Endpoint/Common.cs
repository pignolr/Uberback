﻿using Nancy;

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
    }
}