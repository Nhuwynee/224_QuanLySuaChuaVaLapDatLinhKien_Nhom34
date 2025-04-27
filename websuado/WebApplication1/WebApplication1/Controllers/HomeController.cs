using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        public ActionResult ThongKe()
        {
            ViewBag.Message = "Your application description page.";

            ViewBag.SplineChartData = new List<object>
    {
        new { day = "Mon", income = 100, expense = 80 },
        new { day = "Tue", income = 120, expense = 90 },
        new { day = "Wed", income = 140, expense = 100 },
        new { day = "Thu", income = 160, expense = 120 },
        new { day = "Fri", income = 180, expense = 150 }
    };

            return View();
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}