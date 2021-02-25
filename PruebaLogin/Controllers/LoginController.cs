using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using PruebaLogin.Models;
using PruebaLogin.ClasesAuxiliares;
using Logica;
using DTO;

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
        public string Login(Usuario_CLS oUsuario_CLS)
        {
            LoginLogica loginLogica = new LoginLogica();

            string mensaje = "";

            string nombreUsuario = oUsuario_CLS.nombreUsuario;
            string password = oUsuario_CLS.contra;

            SHA256Managed sha = new SHA256Managed();
            byte[] byteContra = Encoding.Default.GetBytes(password);
            byte[] byteContraCifrada = sha.ComputeHash(byteContra);
            string cadenaContraCifrada = BitConverter.ToString(byteContraCifrada).Replace("-", "");

            UsuarioLogica usuarioLogica = new UsuarioLogica();
            int numeroVeces = usuarioLogica.RegistrosEncontradosNomContra(nombreUsuario, cadenaContraCifrada);

            if (!ModelState.IsValid || numeroVeces == 0)
            {
                var query = (from state in ModelState.Values
                             from error in state.Errors
                             select error.ErrorMessage).ToList();
                mensaje += "<ul class='list-group'><li class='list-group-item'>Error</li>";
                foreach (var item in query)
                {
                    mensaje += "<li class='list-group-item'>" + item + "</li>";
                }
                if (numeroVeces == 0) mensaje += "<li class='list-group-item text-danger'>Usuario o contraseña incorrecto</li>";

                mensaje += "</ul'>";
                return mensaje;
            }
            else
            {

                Session["Usuario"] = usuarioLogica.recuperarUsuarioNomContra(oUsuario_CLS.nombreUsuario, cadenaContraCifrada);
                Session["Permiso"] = loginLogica.Login(oUsuario_CLS);
                mensaje = "1";
            }
            return mensaje;
        }

        public ActionResult Logout()
        {
            Session["Usuario"] = null;
            return RedirectToAction("../PaginaInicio/Index");
        }

        public string recuperarContra(string correo)
        {
            UsuarioLogica usuarioLogica = new UsuarioLogica();
            string rutaLog = Server.MapPath("~/Archivos/LogMails.txt");

            string respuesta = "";

            int cantidad = 0;
            cantidad = usuarioLogica.RegistrosEncontradosEm(correo);
            if (cantidad < 1)
            {
                respuesta = "La dirección de correro no está registrada.";
            }
            else
            {
                respuesta = usuarioLogica.recuperarContra(correo, rutaLog);
            }
            return respuesta;
        }
    }
}