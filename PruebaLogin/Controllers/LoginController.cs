using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using PruebaLogin.Models;

namespace PruebaLogin.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public string Login(UsuarioCLS oUsuarioCLS)
        {
            string mensaje = "";
            if (!ModelState.IsValid)
            {
                var query = (from state in ModelState.Values
                                from error in state.Errors
                                select error.ErrorMessage).ToList();
                mensaje += "<ul class='list-group'>";
                foreach(var item in query)
                {
                    mensaje += "<li class='list-group-item'>"+item+"</li>";
                }
                mensaje += "</ul'>";
                return mensaje;
            }
            else
            {
                string nombreUsuario = oUsuarioCLS.nombreUsuario;
                string password = oUsuarioCLS.contra;

            SHA256Managed sha = new SHA256Managed();
            byte[] byteContra = Encoding.Default.GetBytes(password);
            byte[] byteContraCifrada = sha.ComputeHash(byteContra);
            string cadenContraCifrada = BitConverter.ToString(byteContraCifrada).Replace("-", "");
            using (var bd = new BDDemoLoginEntities())
                {
                int numeroVeces = bd.Usuario.Where(p => p.NOMBREUSUARIO == nombreUsuario
                && p.CONTRA == cadenContraCifrada).Count();
                mensaje = numeroVeces.ToString();
                if (mensaje == "0") mensaje = "<span class='text-danger'>Usuario o contraseña incorrecto</span>";
                else
                {
                    List<MenuCLS> listaMenu = new List<MenuCLS>();
                    Usuario oUsuario = bd.Usuario.Where(p => p.NOMBREUSUARIO == nombreUsuario
                                                            && p.CONTRA == cadenContraCifrada).First();
                    Session["Usuario"] = oUsuario;
                    listaMenu = (from usuario in bd.Usuario
                                            join grupo in bd.Grupo
                                            on usuario.IDGRUPO equals grupo.IDGRUPO
                                            join grupopermiso in bd.GrupoPermiso
                                            on grupo.IDGRUPO equals grupopermiso.IDGRUPO
                                            join permiso in bd.Permiso
                                            on grupopermiso.IDPERMISO equals permiso.IDPERMISO
                                            where grupo.IDGRUPO == oUsuario.IDGRUPO 
                                            && grupopermiso.IDGRUPO == oUsuario.IDGRUPO
                                            && usuario.IDUSUARIO == oUsuario.IDUSUARIO
                                            select new MenuCLS 
                                            {
                                                accion = permiso.NOMBREACCION,
                                                controlador = permiso.NOMBRECONTROLADOR,
                                                pagina = permiso.NOMBREPAGINA
                                            }).ToList();
                    Session["Permiso"] = listaMenu;
                    }
                }
            }
            return mensaje;
        }

        public ActionResult Logout()
        {
            Session["Usuario"] = null;
            return RedirectToAction("../PaginaInicio/Index");
        }
    }
}