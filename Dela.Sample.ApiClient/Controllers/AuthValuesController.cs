using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dela.Sample.ApiClient.Controllers
{
    [Authorize]
    [Route("api/Authorize/[controller]")]
    [ApiController]
    public class AuthValuesController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { $"授权的客户服务: {DateTime.Now.ToString()} {Environment.MachineName} " +
                $"OS: {Environment.OSVersion.VersionString}"};
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }
    }
}