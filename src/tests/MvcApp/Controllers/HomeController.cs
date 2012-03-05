using System.Web.Mvc;

namespace SharpBrake.MvcApp.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult About()
        {
            return View();
        }


        public ActionResult Index()
        {
            this.ViewData["Message"] = "Welcome to ASP.NET MVC!";

            return View();
        }
    }
}