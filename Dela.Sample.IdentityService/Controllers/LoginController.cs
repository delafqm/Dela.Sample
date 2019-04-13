using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dela.Sample.IdentityService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Dela.Sample.IdentityService.Controllers
{
    [Produces("application/json")]
    [Route("api/Login")]
    public class LoginController : Controller
    {
        private IConfiguration configuration;
        public LoginController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        [HttpPost]
        public async Task<ActionResult> RequestToken([FromBody]LoginRequestParam model)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["client_id"] = model.ClientId;
            dict["client_secret"] = configuration[$"IdentityClients:{model.ClientId}:ClientSecret"];
            dict["grant_type"] = configuration[$"IdentityClients:{model.ClientId}:GrantType"];
            dict["username"] = model.UserName;
            dict["password"] = model.Password;

            using (HttpClient http = new HttpClient())
            using (var content = new FormUrlEncodedContent(dict))
            {
                var msg = await http.PostAsync("http://192.168.0.3:8086/connect/token", content);
                if (!msg.IsSuccessStatusCode)
                {
                    return StatusCode(Convert.ToInt32(msg.StatusCode));
                }

                string result = await msg.Content.ReadAsStringAsync();
                return Content(result, "application/json");
            }
        }
    }
}
