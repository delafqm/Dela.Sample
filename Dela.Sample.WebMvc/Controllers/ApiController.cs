using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Dela.Sample.WebMvc.Controllers
{
    public class ApiController : Controller
    {
        private IConfiguration configuration;
        public ApiController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Auth()
        {
            
            string url = $"http://{configuration["ApiGateway:IP"]}:{configuration["ApiGateway:Port"]}/Authorize/ClientService/AuthValues";

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["client_id"] = "client.service.dela";
            dict["client_secret"] = "clientsecret";
            dict["grant_type"] = "client_credentials";

            string response;
            try
            {
                response = await ApiHelper.PostAsync(url, dict);
            }
            catch (Exception ex)
            {
                response = ex.Message;
            }
            ViewData["Message"] = response;

            return View();
        }

        public async Task<IActionResult> Unauth()
        {
            
            string url = $"http://{configuration["ApiGateway:IP"]}:{configuration["ApiGateway:Port"]}/Authorize/ProductService/AuthValues";
            
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["client_id"] = "client.service.dela";
            dict["client_secret"] = "clientsecret";
            dict["grant_type"] = "client_credentials";

            string response;
            try
            {
                response = await ApiHelper.PostAsync(url, dict);
            }
            catch (Exception ex)
            {
                response = ex.Message;
            }
            ViewData["Message"] = response;

            return View();
        }

        public async Task<IActionResult> Gateway()
        {
            return View();
        }
    }
}