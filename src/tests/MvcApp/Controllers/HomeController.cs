using System;
using System.Web.Mvc;

namespace SharpBrake.MvcApp.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            throw new Exception("Test sharpbrake from MVC");
        }
    }
}