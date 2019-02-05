using Nancy;

namespace Uberback.Endpoint
{
    public class Text : NancyModule
    {
        public Text() : base("/text")
        {
            Post("/", x =>
            {
                if (string.IsNullOrEmpty(Request.Query["userId"]) || string.IsNullOrEmpty(Request.Query["flags"]) || string.IsNullOrEmpty(Request.Query["token"]))
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
                Program.P.db.AddTextAsync(Request.Query["flags"], Request.Query["userId"]);
                return (Response.AsJson(new Response.Error()
                {
                    Code = 200,
                    Message = "Ok"
                }));
            });
        }
    }
}
