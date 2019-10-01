using Nancy;
using RethinkDb.Driver.Net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;

namespace Uberback.Endpoint
{
    public class Analyze : NancyModule
    {
        public Analyze() : base("/analyze")
        {
            base.Post("/", x =>
            {
                var args = Common.ParseArgs(Request.Body);

                // Check request
                string error;
                if ((error = Validator.Analyse.ValidateRequest(args)) != null)
                    return (Response.AsJson(new Response.Error() { Message = error }, HttpStatusCode.BadRequest));

                return null;
            });
        }
    }

}
