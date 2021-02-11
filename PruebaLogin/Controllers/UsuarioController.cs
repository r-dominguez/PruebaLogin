using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using PruebaLogin.Models;
using PruebaLogin.Filter;

namespace PruebaLogin.Controllers
{
    [Acceder]
    public class UsuarioController : Controller
    {
        // GET: Usuario 
        public ActionResult Index()
        {
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
                                    nombreGrupo = grupo.NOMBREGRUPO
                                }).ToList();
            }
            return View(listaUsuario);
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

        public ActionResult Agregar()
        {
            listarComboGrupo();
            return View();
        }


        [HttpPost]
        public ActionResult Agregar(UsuarioCLS oUsuarioCLS)
        {
            listarComboGrupo();
            try
            {
                int numRegistroEncontrados = 0;
                using (var bd = new BDDemoLoginEntities())
                {
                    numRegistroEncontrados = bd.Usuario.Where(p => p.NOMBREUSUARIO == oUsuarioCLS.nombreUsuario && p.IDUSUARIO != oUsuarioCLS.idUsuario).Count();
                }
                if (!ModelState.IsValid || numRegistroEncontrados > 1)
                {
                    if (numRegistroEncontrados > 1) oUsuarioCLS.mensajeError = "El nombre de usuario ya existe";
                    return View(oUsuarioCLS);
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
                        oUsuario.HABILITADO = 1;
                        bd.Usuario.Add(oUsuario);
                        bd.SaveChanges();
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
                oUsuarioCLS.idGrupo = (int)oUsuario.IDGRUPO;
            }
            return View(oUsuarioCLS);
        }

        [HttpPost]
        public ActionResult Editar(UsuarioCLS oUsuarioCLS)
        {
            int numRegistroEncontrados = 0;
            using (var bd = new BDDemoLoginEntities())
            {
                numRegistroEncontrados = bd.Usuario.Where(p => p.NOMBREUSUARIO == oUsuarioCLS.nombreUsuario && p.IDUSUARIO != oUsuarioCLS.idUsuario).Count();
            }
            if (!ModelState.IsValid || numRegistroEncontrados >= 1) {
                if (numRegistroEncontrados >= 1) oUsuarioCLS.mensajeError = "El nombre de usuario ya existe";
                listarComboGrupo();
                return View(oUsuarioCLS);
            }
            else
            {
                using (var bd = new BDDemoLoginEntities()) {
                    Usuario oUsuario = bd.Usuario.Where(p => p.IDUSUARIO == oUsuarioCLS.idUsuario).First();
                    oUsuario.NOMBREUSUARIO = oUsuarioCLS.nombreUsuario;
                    oUsuario.IDGRUPO = oUsuarioCLS.idGrupo;
                    bd.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Eliminar(int txtIdUsuario)
        {
            using (var bd = new BDDemoLoginEntities())
            {
                Usuario oUsuario = bd.Usuario.Where(p => p.IDUSUARIO == txtIdUsuario).First();
                oUsuario.HABILITADO = 0;
                bd.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}