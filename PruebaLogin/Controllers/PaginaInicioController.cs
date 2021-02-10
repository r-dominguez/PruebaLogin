using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PruebaLogin.Controllers
{
    public class PaginaInicioController : Controller
    {
        // GET: PaginaInicio
        public ActionResult Index()
        {
            return View();
        }
    }
}