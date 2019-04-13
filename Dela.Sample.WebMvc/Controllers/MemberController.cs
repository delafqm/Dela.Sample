using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dela.Sample.WebMvc.Controllers
{
    public class MemberController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}