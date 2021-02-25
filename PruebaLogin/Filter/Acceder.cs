using DTO;
using PruebaLogin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PruebaLogin.Filter
{
    public class Acceder:ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var usuario = HttpContext.Current.Session["Usuario"];
            List<Menu_CLS> permisos = (List<Menu_CLS>)HttpContext.Current.Session["Permiso"];

            string controlador = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string accion = filterContext.ActionDescriptor.ActionName;
           // int cant = permisos.Where(p => p.accion == accion && p.controlador == controlador).Count();
             int cant = permisos.Where(p => p.controlador == controlador).Count();

            if (usuario == null || cant == 0) 
            {
                filterContext.Result = new RedirectResult("~/PaginaInicio/Index");
            }
            base.OnActionExecuting(filterContext);
        }
    }
}