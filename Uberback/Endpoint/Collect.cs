using Nancy;

namespace Uberback.Endpoint
{
    public class Collect : NancyModule
    {
        public Collect() : base("/collect")
        {
            Post("/", x =>
            {
                if (string.IsNullOrEmpty(Request.Query["type"]) || string.IsNullOrEmpty(Request.Query["token"]))
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
                switch (Request.Query["type"].ToString())
                {
                    case "text":
                        return (Response.AsJson(
                            Program.P.db.GetTextAsync()
                            ));

                    case "image":
                        return (Response.AsJson(
                            Program.P.db.GetTextAsync()
                            ));

                    default:
                        return (Response.AsJson(new Response.Error()
                        {
                            Code = 400,
                            Message = "Type must be text or image"
                        }, HttpStatusCode.BadRequest));
                }
            });
        }
    }
}
