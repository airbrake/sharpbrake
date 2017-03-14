using System;
using System.Web.Mvc;

namespace WebApp.Module.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Throw()
        {
            throw new Exception("Exception from ASP.NET MVC catched by Http module");
        }
    }
}