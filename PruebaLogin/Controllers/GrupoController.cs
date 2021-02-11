using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PruebaLogin.Models;
using System.Transactions;
using PruebaLogin.Filter;

namespace PruebaLogin.Controllers
{
    [Acceder]
    public class GrupoController : Controller
    {
        // GET: Grupo
        public ActionResult Index()
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
            }
            return View(listaGrupo);
        }

        public void listarPermiso()
        {
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
                ViewBag.listaPermiso = new MultiSelectList(listaPermisoCLS, "idPermiso", "nombrePagina");
            }
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
            listarPermiso();
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

            //try
            //{
                int numRegistrosEncontrados = 0;
                using (var bd = new BDDemoLoginEntities())
                {
                    numRegistrosEncontrados = bd.Grupo.Where(p => p.NOMBREGRUPO == oGrupoCLS.nombreGrupo).Count();
                }
                if (!ModelState.IsValid || numRegistrosEncontrados >= 1 || numPermisosSeleccionados<1)
                {
                    if (numRegistrosEncontrados >= 1) oGrupoCLS.mensajeErrorNombre = "El nombre de grupo ya existe ";
                    if (numPermisosSeleccionados < 1) oGrupoCLS.mensajeErrorPermiso = "Debe Seleccionar por lo menos un permiso";
                    listarPermiso();
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
            //}
            //catch(Exception ex)
            //{

            //}
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

        [HttpPost]
        public ActionResult Eliminar(int txtIdGrupo)
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
    }
}