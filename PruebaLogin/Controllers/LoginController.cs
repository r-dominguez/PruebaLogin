using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using PruebaLogin.Models;
using PruebaLogin.ClasesAuxiliares;

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
                                                pagina = permiso.NOMBREPAGINA,
                                                nombreUsuario = usuario.NOMBREUSUARIO
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

        public string recuperarContra(string correo)
        {
            string respuesta = "";
            using (var bd = new BDDemoLoginEntities())
            {
                int cantidad = 0;
                cantidad = bd.Usuario.Where(p => p.EMAIL == correo).Count();
                if (cantidad > 1)
                {
                    respuesta = "La dirección de correro no está registrada.";
                }
                else
                {
                    string rutaLog = Server.MapPath("~/Archivos/LogMails.txt");

                    Random numRa = new Random();
                    int n1 = numRa.Next(0, 9);
                    int n2 = numRa.Next(0, 9);
                    int n3 = numRa.Next(0, 9);
                    int n4 = numRa.Next(0, 9);
                    string nuevaContra = n1.ToString() + n2.ToString() + n3.ToString() + n4.ToString();
                                        SHA256Managed sha = new SHA256Managed();
                    byte[] byteContra = Encoding.Default.GetBytes(nuevaContra);
                    byte[] byteContraCifrada = sha.ComputeHash(byteContra);
                    string cadenContraCifrada = BitConverter.ToString(byteContraCifrada).Replace("-", "");

                    Usuario oUsuario = bd.Usuario.Where(p => p.EMAIL == correo).First();
                    oUsuario.CONTRA = cadenContraCifrada;
                    respuesta = bd.SaveChanges().ToString();

                    string asunto = "Prueba Login - Recuperación de contraseña";
                    string contenido = " <h1>Recuperación de Contraseña</h1><p> Nombre de usuario:" + oUsuario.NOMBREUSUARIO + " </p><p> contraseña: " + nuevaContra + " </p><p> Cordial Saludo! </p> ";

                    CORREO.enviarCorreo(correo, asunto, contenido, rutaLog);

                }
            }
            return respuesta;
        }
    }
}