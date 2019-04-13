using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dela.Sample.WebMvc.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features;

namespace Dela.Sample.WebMvc.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        //端口说明
        public IActionResult Port()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            //string url = "http://192.168.0.3:8087/ClientService/values";

            //Dictionary<string, string> dict = new Dictionary<string, string>();
            //dict["client_id"] = "client.service.dela";
            //dict["client_secret"] = "clientsecret";
            //dict["grant_type"] = "client_credentials";

            //string response;
            //try
            //{
            //    response = await APIHelper.PostAsync(url, dict);
            //}
            //catch (Exception ex)
            //{
            //    response = ex.Message;
            //}
            //ViewData["Message"] = response;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc");
        }

        /// <summary>
        /// GDPR欧盟数据保护法
        /// </summary>
        /// <returns></returns>
        public bool OkCookie()
        {
            var consentFeature = HttpContext.Features.Get<ITrackingConsentFeature>();
            consentFeature.GrantConsent();
            return true;
        }
    }
}
