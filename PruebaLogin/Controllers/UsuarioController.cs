using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using PruebaLogin.Models;
using PruebaLogin.Controllers;
using PruebaLogin.Filter;
using PruebaLogin.ClasesAuxiliares;
using PagedList;
using iTextSharp.text;
using System.IO;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using DTO;
using Logica;

namespace PruebaLogin.Controllers
{
    [Acceder]
    public class UsuarioController : Controller
    {

        public FileResult generarExcel()
        {
            byte[] buffer;

            // recuperar la lista desde la session
            List<Usuario_CLS> lista = (List<Usuario_CLS>)Session["listaUsuario"];
            UsuarioLogica usuarioLogica = new UsuarioLogica();
            buffer = usuarioLogica.datosArchivoExcel(lista);

            return File(buffer, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        public FileResult generarPDF()
        {
            byte[] buffer;

            // recuperar la lista desde la session
            List<Usuario_CLS> lista = (List<Usuario_CLS>)Session["listaUsuario"];
            UsuarioLogica usuarioLogica = new UsuarioLogica();
            buffer = usuarioLogica.datosArchivoPDF(lista);

            return File(buffer, "application/pdf");
        }

        public ActionResult Index(int pagina = 1)
        {
            GrupoLogica grupoLogica = new GrupoLogica();
            ViewBag.listaGrupo = grupoLogica.listaComboGrupo();

            // carga la lista de usuarios paginados desde la capa de logica
            UsuarioLogica usuarioLogica = new UsuarioLogica();
            PaginadorGenericoL<Usuario_CLS> _PaginadorUsuarios = new PaginadorGenericoL<Usuario_CLS>();
            _PaginadorUsuarios = usuarioLogica.listaCompletaPaginada(pagina);

            // sesion con listado de usuario para los archivos pdf y excel
            // el nomUsuario va vacio porque se usa un mismo metodo para listado completo y filtrado
            string nomUsuario = "";
            Session["listaUsuario"] = usuarioLogica.listadoParaArchivos(nomUsuario);

            return View(_PaginadorUsuarios);
        }


        public ActionResult Filtrar(string nombreusuario, int pagina = 1)
        {

            UsuarioLogica usuarioLogica = new UsuarioLogica();
            PaginadorGenericoL<Usuario_CLS> _PaginadorUsuarios = new PaginadorGenericoL<Usuario_CLS>();
            _PaginadorUsuarios = usuarioLogica.listaFiltradaPaginada(nombreusuario, pagina);

            // sesion con listado de usuario para los archivos pdf y excel
            string nomUsuario = "";
            Session["listaUsuario"] = usuarioLogica.listadoParaArchivos(nomUsuario);

            return PartialView("_TablaUsuario", _PaginadorUsuarios);
        }


        public string Guardar(Usuario_CLS oUsuario_CLS, int titulo)
        {
            UsuarioLogica usuarioLogica = new UsuarioLogica();
            string respuesta = "";
            int numRegistroEncontradosUs = 0;
            int numRegistroEncontradosEm = 0;

            if (titulo == -1)
            {
                numRegistroEncontradosUs = usuarioLogica.RegistrosEncontradosUs(oUsuario_CLS.nombreUsuario);
                numRegistroEncontradosUs = usuarioLogica.RegistrosEncontradosEm(oUsuario_CLS.email);
            }
            else
            {
                numRegistroEncontradosUs = usuarioLogica.RegistrosEncontradosUsId(oUsuario_CLS.nombreUsuario, titulo);
                numRegistroEncontradosEm = usuarioLogica.RegistrosEncontradosEmId(oUsuario_CLS.email, titulo);

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
                // llamada a logica 
                respuesta = usuarioLogica.guardar(oUsuario_CLS, titulo);
            }
            return respuesta;
        }

        public string Eliminar(int txtIdUsuario)
        {

            UsuarioLogica usuarioLogica = new UsuarioLogica();
            
            string respuesta = usuarioLogica.eliminar(txtIdUsuario);

            return respuesta;
        }

        public ActionResult CambiarContra(int idUser)
        {
            UsuarioLogica usuarioLogica = new UsuarioLogica();

            Usuario_CLS oUsuario_CLS = usuarioLogica.cambioContraGet(idUser);

            return View(oUsuario_CLS);
        }

        [HttpPost]
        public ActionResult CambiarContra(Usuario_CLS oUsuario_CLS)
        {
            UsuarioLogica usuarioLogica = new UsuarioLogica();

            int numRegistroEncontrados = 0;
            SHA256Managed sha = new SHA256Managed();
            byte[] byteContra = Encoding.Default.GetBytes(oUsuario_CLS.contra);
            byte[] byteContraCifrada = sha.ComputeHash(byteContra);
            string cadenContraCifrada = BitConverter.ToString(byteContraCifrada).Replace("-", "");

            numRegistroEncontrados = usuarioLogica.RegistrosEncontradosContra(oUsuario_CLS.idUsuario, cadenContraCifrada);

            string mensaje = "";
            if (!ModelState.IsValid || numRegistroEncontrados == 0 || oUsuario_CLS.nuevaContra != oUsuario_CLS.confirmaContra
                || (oUsuario_CLS.nuevaContra == null && oUsuario_CLS.confirmaContra == null))
            {
                mensaje += "<ul class='list-group'><li  class='text-danger' style='list-style:none;'>Error</li>";
                var query = (from state in ModelState.Values
                             from error in state.Errors
                             select error.ErrorMessage).ToList();
                mensaje += "<ul class='list-group'>";
                foreach (var item in query)
                {
                    mensaje += "<li class='text-danger' style='list-style:none;'>" + item + "</li>";
                }

                if (numRegistroEncontrados == 0) mensaje += "<li class='text-danger' style='list-style:none;'>La contraseña actual no es correcta</li>";
                if (oUsuario_CLS.nuevaContra != oUsuario_CLS.confirmaContra) mensaje += "<li class='text-danger' style='list-style:none;'>La contraseña nueva y la confirmación son diferentes</li>";
                if (oUsuario_CLS.nuevaContra == null && oUsuario_CLS.confirmaContra == null) mensaje += "<li class='text-danger' style='list-style:none;'>Debe ingresar la nueva contraseña y la confirmación</li>";
                mensaje += "</ul'>";
                oUsuario_CLS.mensajeError = mensaje;
                return View(oUsuario_CLS);

            }
            else
            {
                int respuesta = usuarioLogica.cambioContraPost(oUsuario_CLS);
                if (respuesta == 1)
                {
                    Session["Usuario"] = null;
                    return RedirectToAction("../Login/Index");
                }
                else
                {
                    mensaje = "Ocurrio un error, no se cambio la conrtaseña";
                    oUsuario_CLS.mensajeError = mensaje;
                    return View(oUsuario_CLS);
                }

            }
        }

        public JsonResult RecuperarDatos(int titulo)
        {

            Usuario_CLS oUsuario_CLS = new Usuario_CLS();

            UsuarioLogica usuarioLogica = new UsuarioLogica();

            oUsuario_CLS = usuarioLogica.recuperarUsuario(titulo);

            return Json(oUsuario_CLS, JsonRequestBehavior.AllowGet);
        }
    }
}