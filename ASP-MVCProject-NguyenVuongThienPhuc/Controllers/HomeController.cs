using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ASP_MVCProject_NguyenVuongThienPhuc.Controllers
{
    public class HomeController : Controller
    {
        private cinemaManagerEntities cinemaManagerEntities = new cinemaManagerEntities();

        public ActionResult Index()
        {
            List<movie> movies = cinemaManagerEntities.movies.Take(4).ToList();
            return View(model: movies);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
    }
}