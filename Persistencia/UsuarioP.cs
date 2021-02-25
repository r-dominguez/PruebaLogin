using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DTO;
using Persistencia.MODELS;

namespace Persistencia
{
    public class UsuarioP
    {
        public PaginadorGenericoL<Usuario_CLS> listaUsuarios(int pagina, int _RegistrosPorPagina, int _TotalRegistros, int _TotalPaginas)
        {

            List<Usuario_CLS> lista = new List<Usuario_CLS>();
            using (var bdp = new Persistencia.MODELS.BDLoginDemoEntities())
            {
                lista = (from usuario in bdp.Usuario
                         join grupo in bdp.Grupo
                         on usuario.IDGRUPO equals grupo.IDGRUPO
                         where usuario.HABILITADO == 1
                         select new Usuario_CLS
                         {
                             idUsuario = usuario.IDUSUARIO,
                             nombreUsuario = usuario.NOMBREUSUARIO,
                             email = usuario.EMAIL,
                             nombreGrupo = grupo.NOMBREGRUPO
                         }).OrderBy(p => p.nombreUsuario)
                .Skip((pagina - 1) * _RegistrosPorPagina)
                .Take(_RegistrosPorPagina)
                .ToList();
            }

            PaginadorGenericoL<Usuario_CLS> listaUsuariosPaginados = new PaginadorGenericoL<Usuario_CLS>();
            listaUsuariosPaginados = paginarLista(_RegistrosPorPagina, _TotalRegistros, _TotalPaginas, pagina, lista);

            return listaUsuariosPaginados;
        }
        
       public PaginadorGenericoL<Usuario_CLS> listaUsuariosFiltrados(int pagina, int _RegistrosPorPagina, int _TotalRegistros, int _TotalPaginas, string nomUsuario)
        {

            List<Usuario_CLS> lista = new List<Usuario_CLS>();
            using (var bdp = new BDLoginDemoEntities())
            {
                lista = (from usuario in bdp.Usuario
                         join grupo in bdp.Grupo
                         on usuario.IDGRUPO equals grupo.IDGRUPO
                         where usuario.HABILITADO == 1
                         && usuario.NOMBREUSUARIO.Contains(nomUsuario)
                         select new Usuario_CLS
                         {
                             idUsuario = usuario.IDUSUARIO,
                             nombreUsuario = usuario.NOMBREUSUARIO,
                             email = usuario.EMAIL,
                             nombreGrupo = grupo.NOMBREGRUPO
                         }).OrderBy(p => p.nombreUsuario)
                .Skip((pagina - 1) * _RegistrosPorPagina)
                .Take(_RegistrosPorPagina)
                .ToList();
            }

            PaginadorGenericoL<Usuario_CLS> listaUsuariosPaginados = new PaginadorGenericoL<Usuario_CLS>();
            listaUsuariosPaginados = paginarLista(_RegistrosPorPagina, _TotalRegistros, _TotalPaginas, pagina, lista);

            return listaUsuariosPaginados;
        }



/*
        public int total_usuarios()
        {
            int respuesta = 0;
            using (var bdp = new BDLoginDemoEntities())
            {
                respuesta = bdp.Usuario.Where(p => p.HABILITADO == 1).Count();
            }
            return respuesta;
        }

*/

        public int total_usuariosFiltrados(string filtro)
        {
            int respuesta = 0;
            using (var bdp = new BDLoginDemoEntities())
            {
                if(filtro == "")
                {
                    respuesta = bdp.Usuario.Where(p => p.HABILITADO == 1).Count();
                }
                else
                {
                    respuesta = bdp.Usuario.Where(p => p.HABILITADO == 1 
                    && p.NOMBREUSUARIO.Contains(filtro)).Count();
                }

            }
            return respuesta;
        }

        public PaginadorGenericoL<Usuario_CLS> paginarLista(int _RegistrosPorPagina, int _TotalRegistros, int _TotalPaginas, int pagina, List<Usuario_CLS> lista)
        {
            PaginadorGenericoL<Usuario_CLS> _PaginadorUsuarios;
            _PaginadorUsuarios = new PaginadorGenericoL<Usuario_CLS>()
            {
                RegistrosPorPagina = _RegistrosPorPagina,
                TotalRegistros = _TotalRegistros,
                TotalPaginas = _TotalPaginas,
                PaginaActual = pagina,
                Resultado = lista
            };
            return _PaginadorUsuarios;
        }

        public List<Usuario_CLS> usuarioParaArchivos(string filtro)
        {
            List<Usuario_CLS> lista = new List<Usuario_CLS>();
            if (filtro == null)
            {
                using (var bd = new BDLoginDemoEntities())
                {
                    lista = (from usuario in bd.Usuario
                             join grupo in bd.Grupo
                             on usuario.IDGRUPO equals grupo.IDGRUPO
                             where usuario.HABILITADO == 1
                             select new Usuario_CLS
                             {
                                 idUsuario = usuario.IDUSUARIO,
                                 nombreUsuario = usuario.NOMBREUSUARIO,
                                 email = usuario.EMAIL,
                                 nombreGrupo = grupo.NOMBREGRUPO
                             }).ToList();
                }
            }
            else
            {
                using (var bd = new BDLoginDemoEntities())
                {
                    lista = (from usuario in bd.Usuario
                             join grupo in bd.Grupo
                             on usuario.IDGRUPO equals grupo.IDGRUPO
                             where usuario.HABILITADO == 1
                             && usuario.NOMBREUSUARIO.Contains(filtro)
                             select new Usuario_CLS
                             {
                                 idUsuario = usuario.IDUSUARIO,
                                 nombreUsuario = usuario.NOMBREUSUARIO,
                                 email = usuario.EMAIL,
                                 nombreGrupo = grupo.NOMBREGRUPO
                             }).ToList();
                }
            }
            return lista;
        }

        public string eliminar(int id)
        {
            string respuesta = "";
            try
            {
                using (var bd = new BDLoginDemoEntities())
                {
                    // borrado logico del usuario
                    Usuario oUsuario = bd.Usuario.Where(p => p.IDUSUARIO == id).First();
                    oUsuario.HABILITADO = 0;
                    respuesta = bd.SaveChanges().ToString();
                }
            }
            catch (Exception ex)
            {
                respuesta = "";
            }
            return respuesta;
        }

        public Usuario_CLS cambiarContraGet(int id)
        {
            Usuario_CLS oUsuarioCLS = new Usuario_CLS();
            using (var bd = new BDLoginDemoEntities())
            {
                Usuario oUsuario = bd.Usuario.Where(p => p.IDUSUARIO == id).First();
                oUsuarioCLS.idUsuario = oUsuario.IDUSUARIO;
                oUsuarioCLS.nombreUsuario = oUsuario.NOMBREUSUARIO;

            }
            return oUsuarioCLS;
        }

        public int cambioContraPost(int id, string contraCifrada)
        {
            int respuesta = 0;

            using (var bd = new BDLoginDemoEntities())
            {
                Usuario oUsuario = bd.Usuario.Where(p => p.IDUSUARIO == id).First();
                oUsuario.CONTRA = contraCifrada;
                respuesta = bd.SaveChanges();
            }
            return respuesta;
        }

        public int RegistrosEncontradosContra(int id, string contra)
        {
            int respuesta = 0;
            using (var bd = new BDLoginDemoEntities())
            {
                respuesta = bd.Usuario.Where(p => p.IDUSUARIO == id && p.CONTRA == contra).Count();
            }
            return respuesta;
        }

        public Usuario_CLS recuperarUsuario(int id)
        {
            Usuario_CLS oUsuario_CLS = new Usuario_CLS();
            UsuarioP usuarioP = new UsuarioP();

            using (var bd = new BDLoginDemoEntities())
            {
                Usuario oUsuario = bd.Usuario.Where(p => p.IDUSUARIO == id).First();
                oUsuario_CLS.idUsuario = oUsuario.IDUSUARIO;
                oUsuario_CLS.nombreUsuario = oUsuario.NOMBREUSUARIO;
                oUsuario_CLS.email = oUsuario.EMAIL;
                oUsuario_CLS.idGrupo = (int)oUsuario.IDGRUPO;
            }
            return oUsuario_CLS;
        }

        public int RegistrosEncontradosUs(string nomUsuario)
        {
            int respuesta = 0;
            using (var bd = new BDLoginDemoEntities())
            {
                {
                    respuesta = bd.Usuario.Where(p => p.NOMBREUSUARIO == nomUsuario).Count();
                }
            }
            return respuesta;
        }

        public int RegistrosEncontradosUsId(string nomUsuario, int id)
        {
            int respuesta = 0;
            using (var bd = new BDLoginDemoEntities())
            {
                {
                    respuesta = bd.Usuario.Where(p => p.NOMBREUSUARIO == nomUsuario && p.IDUSUARIO != id).Count();
                }
            }
            return respuesta;
        }

        public int RegistrosEncontradosEm(string emailUsuario)
        {
            int respuesta = 0;
            using (var bd = new BDLoginDemoEntities())
            {
                {
                    respuesta = bd.Usuario.Where(p => p.EMAIL == emailUsuario).Count();
                }
            }
            return respuesta;
        }
        public int RegistrosEncontradosEmId(string emailUsuario, int id)
        {
            int respuesta = 0;
            using (var bd = new BDLoginDemoEntities())
            {
                {
                    respuesta = bd.Usuario.Where(p => p.EMAIL == emailUsuario
                        && p.IDUSUARIO != id).Count();
                }
            }
            return respuesta;
        }

        public string guardar(Usuario_CLS oUsuario_CLS, int id, string cadenaContraCifrada)
        {
            string respuesta = "";
            try { 
                using (var bd = new BDLoginDemoEntities())
                {
                    if (id == -1)
                    {
                        // guardar
                        Usuario oUsuario = new Usuario();
                        oUsuario.NOMBREUSUARIO = oUsuario_CLS.nombreUsuario;
                        oUsuario.CONTRA = cadenaContraCifrada;
                        oUsuario.IDGRUPO = oUsuario_CLS.idGrupo;
                        oUsuario.EMAIL = oUsuario_CLS.email;
                        oUsuario.HABILITADO = 1;
                        bd.Usuario.Add(oUsuario);
                        respuesta = bd.SaveChanges().ToString();
                        if (respuesta == "0") respuesta = "";
                    }
                    else
                    {
                        // editar
                        Usuario oUsuario = bd.Usuario.Where(p => p.IDUSUARIO == id).First();
                        oUsuario.NOMBREUSUARIO = oUsuario_CLS.nombreUsuario;
                        oUsuario.EMAIL = oUsuario_CLS.email;
                        oUsuario.IDGRUPO = oUsuario_CLS.idGrupo;
                        respuesta = bd.SaveChanges().ToString();
                    }
                }

            } catch (Exception ex)
                {
                    respuesta = "";
                }
            return respuesta;
        }

        public int RegistrosEncontradosNomContra(string nom, string contra)
        {
            int respuesta = 0;
            using (var bd = new BDLoginDemoEntities())
            {
                {
                    respuesta = bd.Usuario.Where(p => p.NOMBREUSUARIO == nom && p.CONTRA == contra).Count();
                }
            }
            return respuesta;
        }


        public Usuario_CLS recuperarUsuarioNomContra(string nom, string contra)
        {
            Usuario_CLS oUsuario_CLS = new Usuario_CLS();
            UsuarioP usuarioP = new UsuarioP();

            using (var bd = new BDLoginDemoEntities())
            {
                Usuario oUsuario = bd.Usuario.Where(p => p.NOMBREUSUARIO == nom 
                && p.CONTRA == contra).First();
                oUsuario_CLS.idUsuario = oUsuario.IDUSUARIO;
                oUsuario_CLS.nombreUsuario = oUsuario.NOMBREUSUARIO;
                oUsuario_CLS.email = oUsuario.EMAIL;
                oUsuario_CLS.idGrupo = (int)oUsuario.IDGRUPO;
            }
            return oUsuario_CLS;
        }







        public List<Menu_CLS> recuperarMenus(Usuario_CLS oUsuario_CLS, string cadenaContraCifrada)
        {
            Usuario_CLS oUsuario = new Usuario_CLS();
            List<Menu_CLS> listaMenu = new List<Menu_CLS>();
            using (var bd = new BDLoginDemoEntities())
            {

                //oUsuario = recuperarUsuario(oUsuario_CLS.idUsuario);
                oUsuario = recuperarUsuarioNomContra(oUsuario_CLS.nombreUsuario, cadenaContraCifrada);

                listaMenu = (from usuario in bd.Usuario
                             join grupo in bd.Grupo
                             on usuario.IDGRUPO equals grupo.IDGRUPO
                             join grupopermiso in bd.GrupoPermiso
                             on grupo.IDGRUPO equals grupopermiso.IDGRUPO
                             join permiso in bd.Permiso
                             on grupopermiso.IDPERMISO equals permiso.IDPERMISO
                             where grupo.IDGRUPO == oUsuario.idGrupo
                             && grupopermiso.IDGRUPO == oUsuario.idGrupo
                             && usuario.IDUSUARIO == oUsuario.idUsuario
                             select new Menu_CLS
                             {
                                 accion = permiso.NOMBREACCION,
                                 controlador = permiso.NOMBRECONTROLADOR,
                                 pagina = permiso.NOMBREPAGINA,
                                 nombreUsuario = usuario.NOMBREUSUARIO
                             }).ToList();
            }
            return listaMenu;
        }
        public string actualizarContra(string correo, string cadenContraCifrada)
        {
            string nombre = "";

            using (var bd = new BDLoginDemoEntities())
            {
                Usuario oUsuario = bd.Usuario.Where(p => p.EMAIL == correo).First();

                oUsuario.CONTRA = cadenContraCifrada;

                nombre = oUsuario.NOMBREUSUARIO;

                bd.SaveChanges().ToString();

            }
            return nombre;
        }
    }
}
