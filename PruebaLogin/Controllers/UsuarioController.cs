using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using PruebaLogin.Models;
using PruebaLogin.Filter;
using PruebaLogin.ClasesAuxiliares;

namespace PruebaLogin.Controllers
{
    [Acceder]
    public class UsuarioController : Controller
    {
        // GET: Usuario 
        public ActionResult Index()
        {
            listarComboGrupo();
            List<UsuarioCLS> listaUsuario = new List<UsuarioCLS>();
            using (var bd = new BDDemoLoginEntities())
            {
                listaUsuario = (from usuario in bd.Usuario
                                join grupo in bd.Grupo
                                on usuario.IDGRUPO equals grupo.IDGRUPO
                                where usuario.HABILITADO == 1
                                select new UsuarioCLS
                                {
                                    idUsuario = usuario.IDUSUARIO,
                                    nombreUsuario = usuario.NOMBREUSUARIO,
                                    email = usuario.EMAIL,
                                    nombreGrupo = grupo.NOMBREGRUPO
                                }).ToList();
            }
            return View(listaUsuario);
        }



        public ActionResult Filtrar(UsuarioCLS oUsuarioCLS)
        {
            string nomUsuario = oUsuarioCLS.nombreUsuario;
            List<UsuarioCLS> listaUsuario = new List<UsuarioCLS>();
            using (var bd = new BDDemoLoginEntities())
            {
                if (nomUsuario == null)
                {

                    listaUsuario = (from usuario in bd.Usuario
                                    join grupo in bd.Grupo
                                    on usuario.IDGRUPO equals grupo.IDGRUPO
                                    where usuario.HABILITADO == 1
                                    select new UsuarioCLS
                                    {
                                        idUsuario = usuario.IDUSUARIO,
                                        nombreUsuario = usuario.NOMBREUSUARIO,
                                        email = usuario.EMAIL,
                                        nombreGrupo = grupo.NOMBREGRUPO
                                    }).ToList();
                }else
                {
                    listaUsuario = (from usuario in bd.Usuario
                                    join grupo in bd.Grupo
                                    on usuario.IDGRUPO equals grupo.IDGRUPO
                                    where usuario.HABILITADO == 1 
                                    && usuario.NOMBREUSUARIO.Contains(nomUsuario)
                                    select new UsuarioCLS
                                    {
                                        idUsuario = usuario.IDUSUARIO,
                                        nombreUsuario = usuario.NOMBREUSUARIO,
                                        email = usuario.EMAIL,
                                        nombreGrupo = grupo.NOMBREGRUPO
                                    }).ToList();
                }
            }
            return PartialView("_TablaUsuario", listaUsuario);
        }



        public void listarComboGrupo()
        {
            List<SelectListItem> lista;
            using (var bd = new BDDemoLoginEntities())
            {
                lista = (from grupo in bd.Grupo
                              where grupo.HABILITADO == 1
                              select new SelectListItem
                              {
                                  Text = grupo.NOMBREGRUPO,
                                  Value = grupo.IDGRUPO.ToString()
                              }).ToList();
                lista.Insert(0, new SelectListItem { Text = "Seleccione una opción", Value = "" });
                ViewBag.listaGrupo = lista;
            }

        }


        public string Guardar(UsuarioCLS oUsuarioCLS, int titulo)
        {
            string respuesta = "";

            int numRegistroEncontradosUs = 0;
            int numRegistroEncontradosEm = 0;
            try
            {
                using (var bd = new BDDemoLoginEntities())
                {
                    if (titulo== -1)
                    {
                    numRegistroEncontradosUs = bd.Usuario.Where(p => p.NOMBREUSUARIO == oUsuarioCLS.nombreUsuario).Count();
                    numRegistroEncontradosEm = bd.Usuario.Where(p => p.EMAIL == oUsuarioCLS.email).Count();
                    }
                    else
                    {
                        numRegistroEncontradosUs = bd.Usuario.Where(p => p.NOMBREUSUARIO == oUsuarioCLS.nombreUsuario && p.IDUSUARIO != titulo).Count();
                        numRegistroEncontradosEm = bd.Usuario.Where(p => p.EMAIL == oUsuarioCLS.email
                        && p.IDUSUARIO != titulo).Count();
                    }

                }
                if (!ModelState.IsValid || numRegistroEncontradosUs >= 1 || numRegistroEncontradosEm >= 1)
                {
                    // revisar errores del modelo 
                    var query = (from state in ModelState.Values
                                 from error in state.Errors
                                 select error.ErrorMessage).ToList();
                    // concatenar el mensaje de error
                    respuesta += "<ul class='list-group'>";
                    foreach (var item in query)
                    {
                        respuesta += "<li class='text-danger list-group-item' style='list-style:none;'>" + item + "</li>";
                    }
                    if (numRegistroEncontradosUs >= 1) respuesta += "<li class='text-danger list-group-item' style='list-style:none;'>El nombre de usuario ya existe</li>";
                    if (numRegistroEncontradosEm >= 1) respuesta += "<li class='text-danger list-group-item' style='list-style:none;'>El mail ya existe</li>";
                    respuesta += "</ul'>";
                    //
                }
                else
                {
                    using (var bd = new BDDemoLoginEntities())
                    {
                        if (titulo == -1)
                        {
                            // guardar
                            Usuario oUsuario = new Usuario();
                            oUsuario.NOMBREUSUARIO = oUsuarioCLS.nombreUsuario;
                            SHA256Managed sha = new SHA256Managed();
                            byte[] byteContra = Encoding.Default.GetBytes(oUsuarioCLS.contra);
                            byte[] byteContraCifrada = sha.ComputeHash(byteContra);
                            string cadenContraCifrada = BitConverter.ToString(byteContraCifrada).Replace("-", "");
                            oUsuario.CONTRA = cadenContraCifrada;
                            oUsuario.IDGRUPO = oUsuarioCLS.idGrupo;
                            oUsuario.EMAIL = oUsuarioCLS.email;
                            oUsuario.HABILITADO = 1;
                            bd.Usuario.Add(oUsuario);
                            respuesta = bd.SaveChanges().ToString();
                            if (respuesta == "0") respuesta = "";
                        }
                        else
                        {
                            // editar
                            Usuario oUsuario = bd.Usuario.Where(p => p.IDUSUARIO == titulo).First();
                            oUsuario.NOMBREUSUARIO = oUsuarioCLS.nombreUsuario;
                            oUsuario.EMAIL = oUsuarioCLS.email;
                            oUsuario.IDGRUPO = oUsuarioCLS.idGrupo;
                            respuesta = bd.SaveChanges().ToString();
                        }
                    }
                }
            } catch (Exception ex)
            {
                respuesta = "";
            }
            return respuesta;
        }


        public ActionResult Agregar()
        {
            listarComboGrupo();
            return View();
        }


        [HttpPost]
        public ActionResult Agregar(UsuarioCLS oUsuarioCLS)
        {
            listarComboGrupo();
            int respuesta = 0;
            try
            {
                string mensaje = "";
                int numRegistroEncontradosUs = 0;
                int numRegistroEncontradosEm = 0;
                using (var bd = new BDDemoLoginEntities())
                {
                    numRegistroEncontradosUs = bd.Usuario.Where(p => p.NOMBREUSUARIO == oUsuarioCLS.nombreUsuario).Count();
                    numRegistroEncontradosEm = bd.Usuario.Where(p => p.EMAIL == oUsuarioCLS.email).Count();
                }
                if (!ModelState.IsValid || numRegistroEncontradosUs >= 1 || numRegistroEncontradosEm >= 1)
                {
                    //
                    var query = (from state in ModelState.Values
                                 from error in state.Errors
                                 select error.ErrorMessage).ToList();
                    mensaje += "<ul class='list-group'>";
                    foreach (var item in query)
                    {
                        mensaje += "<li class='text-danger' style='list-style:none;'>" + item + "</li>";
                    }
                    if (numRegistroEncontradosUs >= 1) mensaje += "<li class='text-danger' style='list-style:none;'>El nombre de usuario ya existe</li>";
                    if (numRegistroEncontradosEm >= 1) mensaje += "<li class='text-danger' style='list-style:none;'>El mail ya existe</li>";
                    mensaje += "</ul'>";
                    oUsuarioCLS.mensajeError = mensaje;
                    return View(oUsuarioCLS);
                    //

                }
                else
                {
                    using (var bd = new BDDemoLoginEntities())
                    {
                        Usuario oUsuario = new Usuario();
                        oUsuario.NOMBREUSUARIO = oUsuarioCLS.nombreUsuario;
                        SHA256Managed sha = new SHA256Managed();
                        byte[] byteContra = Encoding.Default.GetBytes(oUsuarioCLS.contra);
                        byte[] byteContraCifrada = sha.ComputeHash(byteContra);
                        string cadenContraCifrada = BitConverter.ToString(byteContraCifrada).Replace("-","");
                        oUsuario.CONTRA = cadenContraCifrada;
                        oUsuario.IDGRUPO = oUsuarioCLS.idGrupo;
                        oUsuario.EMAIL = oUsuarioCLS.email;
                        oUsuario.HABILITADO = 1;
                        bd.Usuario.Add(oUsuario);
                        respuesta = bd.SaveChanges();
                    }
                    if (respuesta ==1)
                    {
                        string rutaLog = Server.MapPath("~/Archivos/LogMails.txt");
                        string contenido = "<h1>BIENVENIDO AL SISTEMA</h1><p>Nombre de usuario:" + oUsuarioCLS.nombreUsuario + "</p><p> contraseña: " + oUsuarioCLS.contra + "</p><p>Cordial Saludo!</p>";
                        string asunto = "Registro de Usuario, bienvenido al sistema";
                        CORREO.enviarCorreo(oUsuarioCLS.email,asunto, contenido, rutaLog);
                    }
                }
            }catch(Exception ex)
            {

            }
            return RedirectToAction("Index");
        }

        public ActionResult Editar(int idUs)
        {
            listarComboGrupo();
            UsuarioCLS oUsuarioCLS = new UsuarioCLS();
            using (var bd = new BDDemoLoginEntities())
            {
                Usuario oUsuario = bd.Usuario.Where(p => p.IDUSUARIO == idUs).First();
                oUsuarioCLS.idUsuario = oUsuario.IDUSUARIO;
                oUsuarioCLS.nombreUsuario = oUsuario.NOMBREUSUARIO;
                oUsuarioCLS.email = oUsuario.EMAIL;
                oUsuarioCLS.idGrupo = (int)oUsuario.IDGRUPO;
            }
            return View(oUsuarioCLS);
        }

        [HttpPost]
        public ActionResult Editar(UsuarioCLS oUsuarioCLS)
        {
            string mensaje = "";
            int numRegistroEncontradosUs = 0;
            int numRegistroEncontradosEm = 0;
            using (var bd = new BDDemoLoginEntities())
            {
                numRegistroEncontradosUs = bd.Usuario.Where(p => p.NOMBREUSUARIO == oUsuarioCLS.nombreUsuario && p.IDUSUARIO != oUsuarioCLS.idUsuario).Count();
                numRegistroEncontradosEm = bd.Usuario.Where(p => p.EMAIL == oUsuarioCLS.email && p.IDUSUARIO != oUsuarioCLS.idUsuario).Count();
            }
            if (!ModelState.IsValid || numRegistroEncontradosUs >= 1 || numRegistroEncontradosEm >= 1) {
                listarComboGrupo();
                //
                var query = (from state in ModelState.Values
                             from error in state.Errors
                             select error.ErrorMessage).ToList();
                mensaje += "<ul class='list-group'>";
                foreach (var item in query)
                {
                    mensaje += "<li class='text-danger' style='list-style:none;'>" + item + "</li>";
                }
                if (numRegistroEncontradosUs >= 1) mensaje += "<li class='text-danger' style='list-style:none;'>El nombre de usuario ya existe</li>";
                if (numRegistroEncontradosEm >= 1) mensaje += "<li class='text-danger' style='list-style:none;'>El mail ya existe</li>";
                mensaje += "</ul'>";
                oUsuarioCLS.mensajeError = mensaje;
                return View(oUsuarioCLS);
                //
            }
            else
            {
                using (var bd = new BDDemoLoginEntities()) {
                    Usuario oUsuario = bd.Usuario.Where(p => p.IDUSUARIO == oUsuarioCLS.idUsuario).First();
                    oUsuario.NOMBREUSUARIO = oUsuarioCLS.nombreUsuario;
                    oUsuario.EMAIL = oUsuarioCLS.email;
                    oUsuario.IDGRUPO = oUsuarioCLS.idGrupo;
                    bd.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult EliminarSinAjax(int txtIdUsuario)
        {
            using (var bd = new BDDemoLoginEntities())
            {
                Usuario oUsuario = bd.Usuario.Where(p => p.IDUSUARIO == txtIdUsuario).First();
                oUsuario.HABILITADO = 0;
                bd.SaveChanges();
            }
            return RedirectToAction("Index");
        }


        public string Eliminar(int txtIdUsuario)
        {
            string respuesta = "";
            try
            {
                using (var bd = new BDDemoLoginEntities())
                {
                    Usuario oUsuario = bd.Usuario.Where(p => p.IDUSUARIO == txtIdUsuario).First();
                    oUsuario.HABILITADO = 0;
                    respuesta = bd.SaveChanges().ToString();
                }
            }catch(Exception ex)
            {
                respuesta = "";
            }
            return respuesta;
        }

        public ActionResult CambiarContra(int idUser)
        {
            int respuesta = 0;
            UsuarioCLS oUsuarioCLS = new UsuarioCLS();

            using (var bd = new BDDemoLoginEntities())
            {
                Usuario oUsuario = bd.Usuario.Where(p => p.IDUSUARIO == idUser).First();
                oUsuarioCLS.idUsuario = oUsuario.IDUSUARIO;
                oUsuarioCLS.nombreUsuario = oUsuario.NOMBREUSUARIO;

            }
                return View(oUsuarioCLS);
        }

        [HttpPost]
        public ActionResult CambiarContra(UsuarioCLS oUsuarioCLS)
        {

            int numRegistroEncontrados = 0;
            SHA256Managed sha = new SHA256Managed();
            byte[] byteContra = Encoding.Default.GetBytes(oUsuarioCLS.contra);
            byte[] byteContraCifrada = sha.ComputeHash(byteContra);
            string cadenContraCifrada = BitConverter.ToString(byteContraCifrada).Replace("-", "");

            using (var bd = new BDDemoLoginEntities())
            {
                numRegistroEncontrados = bd.Usuario.Where(p => p.IDUSUARIO == oUsuarioCLS.idUsuario && p.CONTRA == cadenContraCifrada).Count();
            }

            string mensaje = "";
            if (!ModelState.IsValid || numRegistroEncontrados == 0 || oUsuarioCLS.nuevaContra != oUsuarioCLS.confirmaContra
                || (oUsuarioCLS.nuevaContra == null && oUsuarioCLS.confirmaContra == null))
            {
                var query = (from state in ModelState.Values
                             from error in state.Errors
                             select error.ErrorMessage).ToList();
                mensaje += "<ul class='list-group'>";
                foreach (var item in query)
                {
                    mensaje += "<li class='text-danger' style='list-style:none;'>" + item + "</li>";
                }
                if (numRegistroEncontrados == 0)mensaje += "<li class='text-danger' style='list-style:none;'>La contraseña actual no es correcta</li>";
                if (oUsuarioCLS.nuevaContra != oUsuarioCLS.confirmaContra) mensaje += "<li class='text-danger' style='list-style:none;'>La contraseña nueva y la confirmación son diferentes</li>";
                if(oUsuarioCLS.nuevaContra == null && oUsuarioCLS.confirmaContra == null) mensaje += "<li class='text-danger' style='list-style:none;'>Debe ingresar la nueva contraseña y la confirmación</li>";
                mensaje += "</ul'>";
                oUsuarioCLS.mensajeError = mensaje;
                return View(oUsuarioCLS);
            }
            else
            {
                // actualizo la contraseña
                using (var bd = new BDDemoLoginEntities())
                {
                    Usuario oUsuario = bd.Usuario.Where(p => p.IDUSUARIO == oUsuarioCLS.idUsuario).First();
                    SHA256Managed shaN = new SHA256Managed();
                    byte[] byteContraN = Encoding.Default.GetBytes(oUsuarioCLS.nuevaContra);
                    byte[] byteContraCifradaN = shaN.ComputeHash(byteContraN);
                    string cadenContraCifradaN = BitConverter.ToString(byteContraCifradaN).Replace("-", "");
                    oUsuario.CONTRA = cadenContraCifradaN;
                    bd.SaveChanges();
                }
            }
            Session["Usuario"] = null;
            return RedirectToAction("../Login/Index");
        }

        public JsonResult RecuperarDatos(int titulo)
        {
            UsuarioCLS oUsuarioCLS = new UsuarioCLS();
            using (var bd = new BDDemoLoginEntities())
            {
                Usuario oUsuario = bd.Usuario.Where(p => p.IDUSUARIO == titulo).First();
                oUsuarioCLS.idUsuario = oUsuario.IDUSUARIO;
                oUsuarioCLS.nombreUsuario = oUsuario.NOMBREUSUARIO;
                oUsuarioCLS.email = oUsuario.EMAIL;
                oUsuarioCLS.idGrupo = (int)oUsuario.IDGRUPO;
            }
            return Json(oUsuarioCLS, JsonRequestBehavior.AllowGet);
        }
    }
}