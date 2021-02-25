using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using DTO;
using Persistencia.MODELS;
using System.Transactions;

namespace Persistencia
{
    public class GrupoP
    {
        public List<SelectListItem> listaComboGrupos()
        {
           List<SelectListItem> lista = new List<SelectListItem>();
           using (var bdp = new BDLoginDemoEntities())
            {
                lista = (from grupo in bdp.Grupo
                              where grupo.HABILITADO == 1
                              select new SelectListItem
                              {
                                  Text = grupo.NOMBREGRUPO,
                                  Value = grupo.IDGRUPO.ToString()
                                }).ToList();
                                lista.Insert(0, new SelectListItem { Text = "Seleccione una opción", Value = "" });
            }
            return lista;
        }


        public int total_gruposFiltrado(string filtro)
        {
            int respuesta = 0;
            using (var bd = new BDLoginDemoEntities())
            {
                if (filtro == "")
                {
                    respuesta = bd.Grupo.Where(p => p.HABILITADO == 1).Count();
                }
                else
                {
                    respuesta = bd.Grupo.Where(p => p.HABILITADO == 1
                                    && p.NOMBREGRUPO.Contains(filtro)).Count();
                }
            }
            return respuesta;
        }


        public PaginadorGenericoL<Grupo_CLS> listaGrupos(int pagina, int _RegistrosPorPagina, int _TotalRegistros, int _TotalPaginas)
        {

            List<Grupo_CLS> lista = new List<Grupo_CLS>();
            using (var bd = new BDLoginDemoEntities())
            {
                lista = (from grupo in bd.Grupo
                              where grupo.HABILITADO == 1
                              select new Grupo_CLS
                              {
                                  idGrupo = grupo.IDGRUPO,
                                  nombreGrupo = grupo.NOMBREGRUPO
                              }).OrderBy(p => p.nombreGrupo)
                               .Skip((pagina - 1) * _RegistrosPorPagina)
                               .Take(_RegistrosPorPagina)
                               .ToList();
            }

            PaginadorGenericoL<Grupo_CLS> listaGruposPaginados = new PaginadorGenericoL<Grupo_CLS>();
            listaGruposPaginados = paginarLista(_RegistrosPorPagina, _TotalRegistros, _TotalPaginas, pagina, lista);

            return listaGruposPaginados;
        }


        public PaginadorGenericoL<Grupo_CLS> paginarLista(int _RegistrosPorPagina, int _TotalRegistros, int _TotalPaginas, int pagina, List<Grupo_CLS> lista)
        {
            PaginadorGenericoL<Grupo_CLS> _PaginadorGrupos;
            _PaginadorGrupos = new PaginadorGenericoL<Grupo_CLS>()
            {
                RegistrosPorPagina = _RegistrosPorPagina,
                TotalRegistros = _TotalRegistros,
                TotalPaginas = _TotalPaginas,
                PaginaActual = pagina,
                Resultado = lista
            };
            return _PaginadorGrupos;
        }

        public List<Grupo_CLS> gruposParaArchivos(string filtro)
        {
            List<Grupo_CLS> lista = new List<Grupo_CLS>();
            if (filtro == null)
            {
                using (var bd = new BDLoginDemoEntities())
                {
                    lista = (from grupo in bd.Grupo
                             where grupo.HABILITADO == 1
                             select new Grupo_CLS
                             {
                                 idGrupo = grupo.IDGRUPO,
                                 nombreGrupo = grupo.NOMBREGRUPO
                             }).ToList();
                }
            }
            else
            {
                using (var bd = new BDLoginDemoEntities())
                {
                    lista = (from grupo in bd.Grupo
                             where grupo.HABILITADO == 1
                             && grupo.NOMBREGRUPO.Contains(filtro)
                             select new Grupo_CLS
                             {
                                 idGrupo = grupo.IDGRUPO,
                                 nombreGrupo = grupo.NOMBREGRUPO
                             }).ToList();
                }
            }
            return lista;
        }

        public int cantPermisos(int id)
        {
            int nPermisos = 0;

            using (var bd = new BDLoginDemoEntities())
            {
                nPermisos = bd.GrupoPermiso.Where(p => p.IDGRUPO == id).Count();
            }
            return nPermisos;
        }

        public PaginadorGenericoL<Grupo_CLS> listaGruposFiltrados(int pagina, int _RegistrosPorPagina, int _TotalRegistros, int _TotalPaginas, string nomGrupo)
        {
            List<Grupo_CLS> lista = new List<Grupo_CLS>();
            using (var bd = new BDLoginDemoEntities())
            {
                lista = (from grupo in bd.Grupo
                         where grupo.HABILITADO == 1
                         && grupo.NOMBREGRUPO.Contains(nomGrupo)
                         select new Grupo_CLS
                         {
                             idGrupo = grupo.IDGRUPO,
                             nombreGrupo = grupo.NOMBREGRUPO
                         }).OrderBy(p => p.nombreGrupo)
                               .Skip((pagina - 1) * _RegistrosPorPagina)
                               .Take(_RegistrosPorPagina)
                               .ToList();
            }

            PaginadorGenericoL<Grupo_CLS> listaGruposPaginados = new PaginadorGenericoL<Grupo_CLS>();
            listaGruposPaginados = paginarLista(_RegistrosPorPagina, _TotalRegistros, _TotalPaginas, pagina, lista);

            return listaGruposPaginados;
        }

        public List<Grupo_CLS> listarGruposActivos()
        {
            List<Grupo_CLS> lista = new List<Grupo_CLS>();
            using (var bd = new BDLoginDemoEntities())
            {
                lista = (from grupo in bd.Grupo
                         where grupo.HABILITADO == 1
                         select new Grupo_CLS
                         {
                             idGrupo = grupo.IDGRUPO,
                             nombreGrupo = grupo.NOMBREGRUPO
                         }).ToList();
            }
            return lista;
        }


        public string eliminar (int id)
        {
            string respuesta = "";
            try
            {
                using (var bd = new BDLoginDemoEntities())
                {
                    using (var transaccion = new TransactionScope())
                    {
                        // borrado logico del grupo
                        Grupo oGrupo = bd.Grupo.Where(p => p.IDGRUPO == id).First();
                        oGrupo.HABILITADO = 0;

                        // borra permisos existentes
                        bd.GrupoPermiso.RemoveRange(bd.GrupoPermiso.Where(p => p.IDGRUPO == id));

                        respuesta = bd.SaveChanges().ToString();
                        transaccion.Complete();
                        if (respuesta == "0") respuesta = "";
                        else respuesta = "999";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta = "";
            }
            return respuesta;
        }

        public int RegistrosEncontrados(string nomGrupo, int idGrupo)
        {
            int respuesta = 0;
            using (var bd = new BDLoginDemoEntities())
            {
                {
                    respuesta = bd.Grupo.Where(p => p.NOMBREGRUPO == nomGrupo && p.IDGRUPO != idGrupo).Count();
                    //respuesta = bd.Grupo.Where(p => p.NOMBREGRUPO == nomGrupo).Count();
                }
            }
            return respuesta;
        }

        public string guardar(Grupo_CLS oGrupo_CLS, int titulo, string[] permisosSeleccionados)
        {
            string respuesta = "";

            int numPermisosSeleccionados = permisosSeleccionados.Count();

            using (var bd = new BDLoginDemoEntities())
            {
                using (var transaccion = new TransactionScope())
                {
                    if (titulo == -1)
                    {
                        //guardar
                        Grupo oGrupo = new Grupo();
                        oGrupo.NOMBREGRUPO = oGrupo_CLS.nombreGrupo;
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
                        Grupo oGrupo = bd.Grupo.Where(p => p.IDGRUPO == oGrupo_CLS.idGrupo).First();
                        oGrupo.NOMBREGRUPO = oGrupo_CLS.nombreGrupo;

                        // borra permisos existentes
                        bd.GrupoPermiso.RemoveRange(bd.GrupoPermiso.Where(p => p.IDGRUPO == oGrupo_CLS.idGrupo));

                        // agregar nuevos permisos

                        for (int i = 0; i < numPermisosSeleccionados; i++)
                        {
                            GrupoPermiso oGrupoPermiso = new GrupoPermiso();
                            oGrupoPermiso.IDGRUPO = oGrupo_CLS.idGrupo;
                            oGrupoPermiso.IDPERMISO = int.Parse(permisosSeleccionados[i]);
                            oGrupoPermiso.HABILITADO = 1;
                            bd.GrupoPermiso.Add(oGrupoPermiso);
                        }
                        respuesta = bd.SaveChanges().ToString();
                        transaccion.Complete();
                    }
                }
            }
            return respuesta;
        }

        public List<Grupo_CLS> recuperarCantPermisosGrupo()
        {
            List<Grupo_CLS> lista = new List<Grupo_CLS>();

            using (var bd = new BDLoginDemoEntities())
            {
                lista = (from grupo in bd.Grupo
                         where grupo.HABILITADO == 1
                         select new Grupo_CLS
                         {
                             idGrupo = grupo.IDGRUPO,
                             nombreGrupo = grupo.NOMBREGRUPO
                         }).ToList();
            }
            for (int i = 0; i < lista.Count(); i++)
            {
                lista[i].cantPermisos = cantPermisos(lista[i].idGrupo);
            }

            return lista;
        }

        public Grupo_CLS RecuperarDatosGrupo(int id)
        {
            Grupo_CLS oGrupo_CLS = new Grupo_CLS();
            using (var bd = new BDLoginDemoEntities())
            {
                Grupo oGrupo = bd.Grupo.Where(p => p.IDGRUPO == id).First();
                oGrupo_CLS.idGrupo = oGrupo.IDGRUPO;
                oGrupo_CLS.nombreGrupo = oGrupo.NOMBREGRUPO;
             }
            return oGrupo_CLS;
        }

        public List<SelectListItem> RecuperarPermisoDisponibles()
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            using (var bd = new BDLoginDemoEntities())
            {
                lista = (from permiso in bd.Permiso
                         where permiso.HABILITADO == 1
                         select new SelectListItem
                         {
                             Text = permiso.NOMBREPAGINA,
                             Value = permiso.IDPERMISO.ToString()

                         }).ToList();
            }
            return lista;
        }

        public List<SelectListItem> RecuperarDatosGrupoPermiso(int id)
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            using (var bd = new BDLoginDemoEntities())
            {
                lista = (from grupopermiso in bd.GrupoPermiso
                         join permiso in bd.Permiso
                         on grupopermiso.IDPERMISO equals permiso.IDPERMISO
                         where grupopermiso.IDGRUPO == id
                         select new SelectListItem
                         {
                             Text = permiso.NOMBREPAGINA,
                             Value = permiso.IDPERMISO.ToString()
                         }).ToList();
            }
            return lista;
        }


        public List<Permiso_CLS> RecuperarPermisoEnBase()
        {
            List<Permiso_CLS> lista = new List<Permiso_CLS>();
            
            using (var bd = new BDLoginDemoEntities())
            {
                lista = (from permiso in bd.Permiso
                         where permiso.HABILITADO == 1
                         select new Permiso_CLS
                         {
                             nombrePagina = permiso.NOMBREPAGINA,
                             idPermiso = permiso.IDPERMISO
                         }).ToList();
            }
            return lista;
        }

        public List<GrupoPermiso_CLS> RecuperarPermisosDeGrupo(int id)
        {
            List<GrupoPermiso_CLS> listagp = new List<GrupoPermiso_CLS>();

            using (var bd = new BDLoginDemoEntities())
            {
                listagp = (from grupopermiso in bd.GrupoPermiso
                           join grupo in bd.Grupo
                           on grupopermiso.IDGRUPO equals grupo.IDGRUPO
                           where grupo.IDGRUPO == id
                           select new GrupoPermiso_CLS
                           {
                               idPermiso = (int)grupopermiso.IDPERMISO,
                               idGrupo = (int)grupopermiso.IDGRUPO,
                               idGrupoPermiso = grupopermiso.IDGRUPOPERMISO
                           }).ToList();
            }
            return listagp;
        }
    }
}
