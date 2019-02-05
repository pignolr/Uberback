using Nancy;

namespace Uberback.Endpoint
{
    public class Data : NancyModule
    {
        public Data() : base("/data")
        {
            Post("/", x =>
            {
                if (string.IsNullOrEmpty(Request.Query["type"]) || string.IsNullOrEmpty(Request.Query["userId"]) || string.IsNullOrEmpty(Request.Query["flags"]) || string.IsNullOrEmpty(Request.Query["token"]))
                    return (Response.AsJson(new Response.Error()
                    {
                        Code = 400,
                        Message = "Missing arguments"
                    }, HttpStatusCode.BadRequest));
                if (Request.Query["token"] != Program.P.token)
                    return (Response.AsJson(new Response.Error()
                    {
                        Code = 401,
                        Message = "Bad token"
                    }, HttpStatusCode.Unauthorized));
                switch (Request.Query["type"])
                {
                    case "text":
                        Program.P.db.AddTextAsync(Request.Query["flags"], Request.Query["userId"]);
                        break;

                    case "image":
                        Program.P.db.AddImageAsync(Request.Query["flags"], Request.Query["userId"]);
                        break;

                    default:
                        return (Response.AsJson(new Response.Error()
                        {
                            Code = 400,
                            Message = "Type must be text or image"
                        }, HttpStatusCode.BadRequest));
                }
                return (Response.AsJson(new Response.Error()
                {
                    Code = 200,
                    Message = "Ok"
                }));
            });
        }
    }
}
