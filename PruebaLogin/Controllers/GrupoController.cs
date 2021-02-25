using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PruebaLogin.Models;
using System.Transactions;
using PruebaLogin.Filter;
using PagedList;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using PruebaLogin.ClasesAuxiliares;
using Logica;
using DTO;

namespace PruebaLogin.Controllers
{
    [Acceder]
    public class GrupoController : Controller
    {
        public FileResult generarExcel()
        {
            byte[] buffer;

            // recuperar la lista desde la session
            List<Grupo_CLS> lista = (List<Grupo_CLS>)Session["listaGrupo"];

            GrupoLogica grupoLogica = new GrupoLogica();
            buffer = grupoLogica.datosArchivoExcel(lista);

            return File(buffer, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        // GET: Grupo 
        public ActionResult Index(int pagina = 1)
        {

            GrupoLogica grupoLogica = new GrupoLogica();

            PaginadorGenericoL<Grupo_CLS> _PaginadorGrupos = new PaginadorGenericoL<Grupo_CLS>();
            _PaginadorGrupos = grupoLogica.listaCompletaPaginada(pagina);

            // sesion con listado de usuario para los archivos pdf y excel
            // el nomUsuario va vacio porque se usa un mismo metodo para listado completo y filtrado
            string nomGrupo = "";
            Session["listaGrupo"] = grupoLogica.listadoParaArchivos(nomGrupo);

            return View(_PaginadorGrupos);
        }

        public ActionResult Filtrar(string nombregrupo, int pagina = 1)
        {

            GrupoLogica grupoLogica = new GrupoLogica();
            PaginadorGenericoL<Grupo_CLS> _PaginadorGrupos = new PaginadorGenericoL<Grupo_CLS>();
            _PaginadorGrupos = grupoLogica.listaFiltradaPaginada(nombregrupo, pagina);

            // sesion con listado de usuario para los archivos pdf y excel
            Session["listaGrupo"] = grupoLogica.listadoParaArchivos(nombregrupo);

            return PartialView("_TablaGrupo", _PaginadorGrupos);
        }


        public string Guardar(Grupo_CLS oGrupo_CLS, int titulo, FormCollection form)
        {
            GrupoLogica grupoLogica = new GrupoLogica();
            string respuesta = "";
            // validar que haya seleccionado permisos
            string[] permisosSeleccionados = null;
            if (form["permisosSeleccionadosV"] != null)
            {
                permisosSeleccionados = form["permisosSeleccionadosV"].Split(',');
            }
            // validar que el usuario no se repita
            int numPermisosSeleccionados = 0;
            if (permisosSeleccionados != null) numPermisosSeleccionados = permisosSeleccionados.Count();

            int numRegistrosEncontrados = grupoLogica.RegistrosEncontrados(oGrupo_CLS.nombreGrupo, oGrupo_CLS.idGrupo);;

            if (!ModelState.IsValid || numRegistrosEncontrados >= 1 || numPermisosSeleccionados < 1)
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
                if (numRegistrosEncontrados >= 1) respuesta += "<li class='text-danger list-group-item' style='list-style:none;'>El nombre de grupo ya existe</li>";
                if (numPermisosSeleccionados < 1) respuesta += "<li class='text-danger list-group-item' style='list-style:none;'>Debe Seleccionar por lo menos un permiso </li>";
                respuesta += "</ul'>";
                //
            }
            else
            {
                respuesta = grupoLogica.guardar(oGrupo_CLS, titulo, permisosSeleccionados);

                if (respuesta == "0") respuesta = "";
                else respuesta = "999";
            }
         return respuesta;
        }

        public String Eliminar(int txtIdGrupo)
        {
            string respuesta = "";

            GrupoLogica grupoLogica = new GrupoLogica();
            respuesta = grupoLogica.eliminar(txtIdGrupo);

            return respuesta;
        }

        public JsonResult RecuperarDatosGrupo(int titulo)
        {
            Grupo_CLS oGrupo_CLS = new Grupo_CLS();

            GrupoLogica grupoLogica = new GrupoLogica();

            oGrupo_CLS = grupoLogica.RecuperarDatosGrupo(titulo);

            return Json(oGrupo_CLS, JsonRequestBehavior.AllowGet);
        }


        public JsonResult RecuperarPermisoDisponibles()
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            GrupoLogica grupoLogica = new GrupoLogica();

            lista = grupoLogica.RecuperarPermisoDisponibles();

            return Json(lista, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RecuperarDatosGrupoPermisoSinAsignar(int titulo)
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            GrupoLogica grupoLogica = new GrupoLogica();
            lista = grupoLogica.RecuperarDatosGrupoPermisoSinAsignar(titulo);

            return Json(lista, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RecuperarDatosGrupoPermiso(int titulo)
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            GrupoLogica grupoLogica = new GrupoLogica();

            lista = grupoLogica.RecuperarDatosGrupoPermiso(titulo);

            return Json(lista, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Reportes()
        {
            listarGruposActivos();
            return View();
        }

        public JsonResult gruposAgraficar(int[] grupoParaGraficar)
        {
            List<Grupo_CLS> lista = new List<Grupo_CLS>();
            GrupoLogica grupoLogica = new GrupoLogica();

            lista = grupoLogica.gruposAgraficar(grupoParaGraficar);


            return Json(lista, JsonRequestBehavior.AllowGet);
        }

        // lista grupos y cantidad de permisos por grupo para graficar
        public JsonResult recuperarCantPermisosGrupo()
        {
            List<Grupo_CLS> lista = new List<Grupo_CLS>();

            GrupoLogica grupoLogica = new GrupoLogica();

            lista = grupoLogica.recuperarCantPermisosGrupo();

            return Json(lista, JsonRequestBehavior.AllowGet);
        }
        
        // viewbag de grupos activos
        public void listarGruposActivos()
        {
            List<Grupo_CLS> lista = new List<Grupo_CLS>();

            GrupoLogica grupoLogica = new GrupoLogica();
            lista = grupoLogica.listarGruposActivos();

            ViewBag.listaGrupo = lista;
        }
    }
}