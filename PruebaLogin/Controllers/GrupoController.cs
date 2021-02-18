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

namespace PruebaLogin.Controllers
{
    [Acceder]
    public class GrupoController : Controller
    {
        public FileResult generarExcel()
        {
            byte[] buffer;
            using (MemoryStream ms = new MemoryStream())
            {
                //doc excel 
                ExcelPackage ep = new ExcelPackage();
                // definir hoja de excel 
                ep.Workbook.Worksheets.Add("Reporte de Grupos");
                //agrega hoja al documento
                var currentSheet = ep.Workbook.Worksheets;
                var ew = currentSheet.First();
                // definir nombres de las columnas 
                ew.Cells[1, 1].Value = "Id Grupo";
                ew.Cells[1, 2].Value = "Nombre Grupo";
                ew.Cells[1, 2].Value = "Cant Permisos";
                ew.Column(1).Width = 10;
                ew.Column(2).Width = 30;
                ew.Column(3).Width = 10;
                using (var range = ew.Cells[1, 1, 1, 3])
                {
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Font.Color.SetColor(Color.White);
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkRed);
                }
                // recuperar la lista desde la session
                List<GrupoCLS> lista = (List<GrupoCLS>)Session["listaGrupo"];
                int nregistros = lista.Count();
                //recorrer la lista y cargar los datos en las celdas
                for (int i = 0; i < nregistros; i++)
                {
                    ew.Cells[i + 2, 1].Value = lista[i].idGrupo;
                    ew.Cells[i + 2, 2].Value = lista[i].nombreGrupo;
                    ew.Cells[i + 2, 3].Value = cantPermisos(lista[i].idGrupo);
                }
                ep.SaveAs(ms);
                buffer = ms.ToArray();

            }
            return File(buffer, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        public int cantPermisos(int id)
        {
            int nPermisos = 0;
            using (var bd = new BDDemoLoginEntities())
            {
                nPermisos = bd.GrupoPermiso.Where(p => p.IDGRUPO == id ).Count();
            }
            return nPermisos;
        }
        // GET: Grupo
        public ActionResult Index(int? page)
        {
            List<GrupoCLS> listaGrupo = new List<GrupoCLS>();
            using (var bd = new BDDemoLoginEntities())
            {
                listaGrupo = (from grupo in bd.Grupo
                              where grupo.HABILITADO == 1
                              select new GrupoCLS
                              {
                                  idGrupo = grupo.IDGRUPO,
                                  nombreGrupo = grupo.NOMBREGRUPO
                              }).ToList();
            Session["listaGrupo"] = listaGrupo;
            }
            int pageSize = 5;
            // si page es null le asigna 1 a pageNumber
            int pageNumber = page ?? 1;
            return View(listaGrupo.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Filtrar(GrupoCLS oGrupoCLS)
        {
            string nomGrupo = oGrupoCLS.nombreGrupo;
            List<GrupoCLS> listaGrupo = new List<GrupoCLS>();
            using (var bd = new BDDemoLoginEntities())
            {
                if (nomGrupo == null)
                {
                    listaGrupo = (from grupo in bd.Grupo
                                  where grupo.HABILITADO == 1
                                  select new GrupoCLS
                                  {
                                      idGrupo = grupo.IDGRUPO,
                                      nombreGrupo = grupo.NOMBREGRUPO
                                  }).ToList();
                Session["listaGrupo"] = listaGrupo;
                }
                else
                {
                    listaGrupo = (from grupo in bd.Grupo
                                  where grupo.HABILITADO == 1
                                  && grupo.NOMBREGRUPO.Contains(nomGrupo)
                                  select new GrupoCLS
                                  {
                                      idGrupo = grupo.IDGRUPO,
                                      nombreGrupo = grupo.NOMBREGRUPO
                                  }).ToList();

                    Session["listaGrupo"] = listaGrupo;
                }
            }
            return PartialView("_TablaGrupo", listaGrupo);
        }

        public string Guardar(GrupoCLS oGrupoCLS, int titulo, FormCollection form)
        {
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

            int numRegistrosEncontrados = 0;
            using (var bd = new BDDemoLoginEntities())
            {
                numRegistrosEncontrados = bd.Grupo.Where(p => p.NOMBREGRUPO == oGrupoCLS.nombreGrupo
                && p.IDGRUPO != oGrupoCLS.idGrupo).Count();
            }
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
                using (var bd = new BDDemoLoginEntities())
                {
                    using (var transaccion = new System.Transactions.TransactionScope())
                    {
                        if (titulo == -1) 
                        {
                            //guardar
                            Grupo oGrupo = new Grupo();
                            oGrupo.NOMBREGRUPO = oGrupoCLS.nombreGrupo;
                            oGrupo.HABILITADO = 1;
                            bd.Grupo.Add(oGrupo);

                            for (int i = 0; i < numPermisosSeleccionados; i++)
                            {
                                GrupoPermiso oGrupoPermiso = new GrupoPermiso();
                                oGrupoPermiso.IDGRUPO = oGrupo.IDGRUPO;
                                oGrupoPermiso.IDPERMISO = int.Parse(permisosSeleccionados[i]);
                                oGrupoPermiso.HABILITADO = 1;
                                bd.GrupoPermiso.Add(oGrupoPermiso);
                            }
                            respuesta = bd.SaveChanges().ToString();
                            transaccion.Complete();
                            if (respuesta == "0") respuesta = "";
                            else respuesta = "999";
                        }
                        else
                        {
                            //editar
                            Grupo oGrupo = bd.Grupo.Where(p => p.IDGRUPO == oGrupoCLS.idGrupo).First();
                            oGrupo.NOMBREGRUPO = oGrupoCLS.nombreGrupo;

                            // borra permisos existentes
                            bd.GrupoPermiso.RemoveRange(bd.GrupoPermiso.Where(p => p.IDGRUPO == oGrupoCLS.idGrupo));

                            // agregar nuevos permisos

                            for (int i = 0; i < numPermisosSeleccionados; i++)
                            {
                                GrupoPermiso oGrupoPermiso = new GrupoPermiso();
                                oGrupoPermiso.IDGRUPO = oGrupoCLS.idGrupo;
                                oGrupoPermiso.IDPERMISO = int.Parse(permisosSeleccionados[i]);
                                oGrupoPermiso.HABILITADO = 1;
                                bd.GrupoPermiso.Add(oGrupoPermiso);
                            }
                            respuesta = bd.SaveChanges().ToString();
                            transaccion.Complete();
                            if (respuesta == "0") respuesta = "";
                            else respuesta = "999";
                        }
                    }

                }
            }
                return respuesta;
        }

        public void listarPermisoSelecionados(List<GrupoPermisoCLS> listaPermisosGrupo)
        {
            int[] sListaPermisosGrupo = new int[listaPermisosGrupo.Count()];

            for (int i = 0 ; i < listaPermisosGrupo.Count(); i++) {
                sListaPermisosGrupo[i] = listaPermisosGrupo[i].idPermiso;
            }
            List<PermisoCLS> listaPermisoCLS = new List<PermisoCLS>();

            using (var bd = new BDDemoLoginEntities())
            {
                listaPermisoCLS = (from permiso in bd.Permiso
                                   where permiso.HABILITADO == 1
                                   select new PermisoCLS
                                   {
                                       idPermiso = permiso.IDPERMISO,
                                       nombrePagina = permiso.NOMBREPAGINA
                                   }).ToList();
                ViewBag.listaPermiso = new MultiSelectList(listaPermisoCLS, "idPermiso", "nombrePagina", sListaPermisosGrupo);
            }
        }

        public ActionResult Agregar()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Agregar(GrupoCLS oGrupoCLS, FormCollection form)
        {
            string[] permisosSeleccionados = null;
            if (form["permisos"] != null)
            {
            permisosSeleccionados = form["permisos"].Split(',');
            }

            int numPermisosSeleccionados = 0;
            if (permisosSeleccionados != null)numPermisosSeleccionados = permisosSeleccionados.Count();

            int numRegistrosEncontrados = 0;
            using (var bd = new BDDemoLoginEntities())
            {
                numRegistrosEncontrados = bd.Grupo.Where(p => p.NOMBREGRUPO == oGrupoCLS.nombreGrupo).Count();
            }
            if (!ModelState.IsValid || numRegistrosEncontrados >= 1 || numPermisosSeleccionados<1)
            {
                if (numRegistrosEncontrados >= 1) oGrupoCLS.mensajeErrorNombre = "El nombre de grupo ya existe ";
                if (numPermisosSeleccionados < 1) oGrupoCLS.mensajeErrorPermiso = "Debe Seleccionar por lo menos un permiso";
                return View(oGrupoCLS);
            }
            else
            {
                using (var bd = new BDDemoLoginEntities()) 
                {
                    using (var transaccion = new System.Transactions.TransactionScope()) { 
                        Grupo oGrupo = new Grupo();
                        oGrupo.NOMBREGRUPO = oGrupoCLS.nombreGrupo;
                        oGrupo.HABILITADO = 1;
                        bd.Grupo.Add(oGrupo);

                    for (int i=0; i < numPermisosSeleccionados;i++)
                    {
                        GrupoPermiso oGrupoPermiso = new GrupoPermiso();
                        oGrupoPermiso.IDGRUPO = oGrupoCLS.idGrupo;
                        oGrupoPermiso.IDPERMISO = int.Parse(permisosSeleccionados[i]);
                        oGrupoPermiso.HABILITADO = 1;
                        bd.GrupoPermiso.Add(oGrupoPermiso);
                    }
                    bd.SaveChanges();
                    transaccion.Complete();
                    }
                            
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Editar(int idgrupo)
        {
            GrupoCLS oGrupoCLS = new GrupoCLS();
            using (var bd = new BDDemoLoginEntities())
            {
                Grupo oGrupo = bd.Grupo.Where(p => p.IDGRUPO == idgrupo).First();
                oGrupoCLS.idGrupo = oGrupo.IDGRUPO;
                oGrupoCLS.nombreGrupo = oGrupo.NOMBREGRUPO;
            }
            listarPermisoSelecionados(listaPermisosGrupo(idgrupo));
            return View(oGrupoCLS);
        }

        // recupero los permisos de un grupo 
        public List<GrupoPermisoCLS> listaPermisosGrupo(int idgrupo)
        {
            List<GrupoPermisoCLS> lista = new List<GrupoPermisoCLS>();
            using (var bd = new BDDemoLoginEntities())
            {
                lista = (from grupopermiso in bd.GrupoPermiso
                         join permiso in bd.Permiso
                         on grupopermiso.IDPERMISO equals permiso.IDPERMISO
                         where grupopermiso.IDGRUPO == idgrupo
                         select new GrupoPermisoCLS
                         {
                             idPermiso = (int)grupopermiso.IDPERMISO,
                             nombrePagina = permiso.NOMBREPAGINA
                         }).ToList();

            }
            return (lista);
        }


        [HttpPost]
        public ActionResult Editar(GrupoCLS oGrupoCLS, FormCollection form) 
        {
            string[] permisosSeleccionados = null;
            if (form["permisos"] != null)
            {
                permisosSeleccionados = form["permisos"].Split(',');
            }

            int numPermisosSeleccionados = 0;
            if (permisosSeleccionados != null) numPermisosSeleccionados = permisosSeleccionados.Count();

            //try
            //{
            int numRegistrosEncontrados = 0;
            using (var bd = new BDDemoLoginEntities())
            {
                numRegistrosEncontrados = bd.Grupo.Where(p => p.NOMBREGRUPO == oGrupoCLS.nombreGrupo
                                                            && p.IDGRUPO!=oGrupoCLS.idGrupo).Count();
            }
            if (!ModelState.IsValid || numRegistrosEncontrados >= 1 || numPermisosSeleccionados < 1)
            {
                if (numRegistrosEncontrados >= 1) oGrupoCLS.mensajeErrorNombre = "El nombre de grupo ya existe ";
                if (numPermisosSeleccionados < 1) oGrupoCLS.mensajeErrorPermiso = "Debe Seleccionar por lo menos un permiso";
                listarPermisoSelecionados(listaPermisosGrupo(oGrupoCLS.idGrupo));
                return View(oGrupoCLS);
            }
            else
            {
                using (var bd = new BDDemoLoginEntities())
                {
                    using (var transaccion = new System.Transactions.TransactionScope())
                    {
                        Grupo oGrupo = bd.Grupo.Where(p => p.IDGRUPO == oGrupoCLS.idGrupo).First();
                        oGrupo.NOMBREGRUPO = oGrupoCLS.nombreGrupo;

                        // borra permisos existentes
                        bd.GrupoPermiso.RemoveRange(bd.GrupoPermiso.Where(p => p.IDGRUPO == oGrupoCLS.idGrupo));

                        // agregar nuevos permisos

                        for (int i = 0; i < numPermisosSeleccionados; i++)
                        {
                            GrupoPermiso oGrupoPermiso = new GrupoPermiso();
                            oGrupoPermiso.IDGRUPO = oGrupoCLS.idGrupo;
                            oGrupoPermiso.IDPERMISO = int.Parse(permisosSeleccionados[i]);
                            oGrupoPermiso.HABILITADO = 1;
                            bd.GrupoPermiso.Add(oGrupoPermiso);
                        }
                        bd.SaveChanges();
                        transaccion.Complete();
                    }

                }
            }
            return RedirectToAction("Index");
        }


        public String Eliminar(int? txtIdGrupo)
        {
            string respuesta = "";
            try
            {
                using (var bd = new BDDemoLoginEntities())
                {
                    using (var transaccion = new System.Transactions.TransactionScope())
                    {
                        // borrado logico del grupo
                        Grupo oGrupo = bd.Grupo.Where(p => p.IDGRUPO == txtIdGrupo).First();
                        oGrupo.HABILITADO = 0;

                        // borra permisos existentes
                        bd.GrupoPermiso.RemoveRange(bd.GrupoPermiso.Where(p => p.IDGRUPO == txtIdGrupo));

                        respuesta = bd.SaveChanges().ToString();
                        transaccion.Complete();
                        if (respuesta == "0") respuesta = "";
                        else respuesta = "999";
                    }
                }
            }
            catch(Exception ex)
            {
                respuesta = "";
            }
            return respuesta;
        }


        [HttpPost]
        public ActionResult EliminarSinAjax(int? txtIdGrupo)
        {
            using (var bd = new BDDemoLoginEntities())
            {
                using (var transaccion = new System.Transactions.TransactionScope()) 
                { 
                    Grupo oGrupo = bd.Grupo.Where(p => p.IDGRUPO == txtIdGrupo).First();
                    oGrupo.HABILITADO = 0;

                    // borra permisos existentes
                    int cantPermisos = bd.GrupoPermiso.Where(p=>p.IDGRUPO == txtIdGrupo).Count();

                    for (int i = 0; i < cantPermisos; i++)
                    {
                        GrupoPermiso oGrupoPermiso = bd.GrupoPermiso.Where(p => p.IDGRUPO == txtIdGrupo).First();
                        oGrupoPermiso.HABILITADO = 0;
                    }
                    bd.SaveChanges();
                    transaccion.Complete();
                }
            }
            return RedirectToAction("Index");
        }


        public JsonResult RecuperarDatosGrupo(int titulo)
        {
            GrupoCLS oGrupoCLS = new GrupoCLS();
            using (var bd = new BDDemoLoginEntities())
            {
                Grupo oGrupo = bd.Grupo.Where(p => p.IDGRUPO == titulo).First();
                oGrupoCLS.idGrupo = oGrupo.IDGRUPO;
                oGrupoCLS.nombreGrupo = oGrupo.NOMBREGRUPO;
            }
            return Json(oGrupoCLS, JsonRequestBehavior.AllowGet);
        }


        public JsonResult RecuperarPermisoDisponibles()
        {
            List<SelectListItem> lista = new List<SelectListItem>();
            using (var bd = new BDDemoLoginEntities())
            {
                lista = (from permiso in bd.Permiso
                         where permiso.HABILITADO == 1
                         select new SelectListItem
                         { 
                             Text = permiso.NOMBREPAGINA,
                             Value = permiso.IDPERMISO.ToString()

                         }).ToList();
            }
                return Json(lista, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RecuperarDatosGrupoPermisoSinAsignar(int titulo)
        {
            List<PermisoCLS> listaP = new List<PermisoCLS>();
            using (var bd = new BDDemoLoginEntities())
            {
                listaP = (from permiso in bd.Permiso
                         where permiso.HABILITADO == 1
                         select new PermisoCLS
                         {
                             idPermiso = permiso.IDPERMISO,
                             nombrePagina = permiso.NOMBREPAGINA
                         }).ToList();

            }
            List<GrupoPermisoCLS> listagp = new List<GrupoPermisoCLS>();
            using (var bd = new BDDemoLoginEntities())
            {
                listagp = (from grupopermiso in bd.GrupoPermiso
                         join grupo in bd.Grupo
                         on grupopermiso.IDGRUPO equals grupo.IDGRUPO
                         where grupo.IDGRUPO == titulo
                         select new GrupoPermisoCLS
                         {
                             idPermiso = (int)grupopermiso.IDPERMISO,
                             idGrupo = (int)grupopermiso.IDGRUPO,
                             idGrupoPermiso = grupopermiso.IDGRUPOPERMISO
                         }).ToList();
            }
            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (PermisoCLS p in listaP)
            {
                int cantSi = 0;
                foreach(GrupoPermisoCLS gp in listagp)
                {
                    if (p.idPermiso == gp.idPermiso)
                    {
                        cantSi += 1;
                    }
                }
                if (cantSi == 0)
                {
                    string text = p.nombrePagina;
                    string value = p.idPermiso.ToString();
                    lista.Insert(0, new SelectListItem { Text = text, Value = value });
                }
            }
            return Json(lista, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RecuperarDatosGrupoPermiso(int titulo)
        {
            List<SelectListItem> lista;
            using (var bd = new BDDemoLoginEntities())
            {

                lista = (from grupopermiso in bd.GrupoPermiso
                         join permiso in bd.Permiso
                         on grupopermiso.IDPERMISO equals permiso.IDPERMISO
                         where grupopermiso.IDGRUPO == titulo
                         select new SelectListItem
                         {
                             Text = permiso.NOMBREPAGINA,
                             Value = permiso.IDPERMISO.ToString()
                         }).ToList();
            }
            return Json(lista, JsonRequestBehavior.AllowGet);
        }


    }
}