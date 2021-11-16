using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        public ActionResult<Dictionary<string, string>> Index()
        {
            var result = new Dictionary<string, string>
            {
                ["_.now"] = DateTime.UtcNow.ToString("O"),
                ["_.machine"] = Environment.MachineName,
                ["_.controller"] = nameof(ApiController),
                ["verb"] = Request.Method,
                ["path"] = Request.Path
            };

            foreach (var header in Request.Headers.OrderBy(o => o.Key))
            {
                result[$"headers.{header.Key}"] = header.Value;
            }

            foreach (var cookie in Request.Cookies.OrderBy(o => o.Key))
            {
                result[$"cookies.{cookie.Key}"] = cookie.Value;
            }

            return result;
        }
    }
}
