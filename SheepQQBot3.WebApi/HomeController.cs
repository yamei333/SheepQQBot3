using System.Web.Http;

namespace SheepQQBot3.WebApi
{
    [RoutePrefix("api/home")]
    public class HomeController : ApiController
    {
        [Route("echo")]
        [HttpGet]
        public IHttpActionResult Echo(string name)
            => Json(new { Name = name, Message = $"Hello,{name},action" });
    }
}