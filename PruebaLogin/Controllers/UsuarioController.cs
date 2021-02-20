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

namespace PruebaLogin.Controllers
{
    [Acceder]
    public class UsuarioController : Controller
    {

        public FileResult generarExcel()
        {
            byte[] buffer;
            using (MemoryStream ms = new MemoryStream())
            {
                //doc excel 
                ExcelPackage ep = new ExcelPackage();
                // definir hoja de excel 
                ep.Workbook.Worksheets.Add("Reporte de Usuarios");
                //agrega hoja al documento
                var currentSheet = ep.Workbook.Worksheets;
                var ew = currentSheet.First();
                // definir nombres de las columnas 
                ew.Cells[1, 1].Value = "Id Marca";
                ew.Cells[1, 2].Value = "Nombre Marca";
                ew.Cells[1, 3].Value = "Email Marca";
                ew.Cells[1, 4].Value = "Grupo Marca";
                ew.Column(1).Width = 10;
                ew.Column(2).Width = 20;
                ew.Column(3).Width = 40;
                ew.Column(4).Width = 20;
                using (var range = ew.Cells[1,1,1,4])
                {
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Font.Color.SetColor(Color.White);
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkRed);
                }
                // recuperar la lista desde la session
                List<UsuarioCLS> lista = (List<UsuarioCLS>)Session["listaUsuario"];
                int nregistros = lista.Count();
                //recorrer la lista y cargar los datos en las celdas
                for (int i = 0; i < nregistros; i++)
                {
                    ew.Cells[i + 2, 1].Value = lista[i].idUsuario;
                    ew.Cells[i + 2, 2].Value = lista[i].nombreUsuario;
                    ew.Cells[i + 2, 3].Value = lista[i].email;
                    ew.Cells[i + 2, 4].Value = lista[i].nombreGrupo;
                }

                ep.SaveAs(ms);
                buffer = ms.ToArray();

            }
            return File(buffer, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }


        public FileResult generarPDF()
        {
            Document doc = new Document();
            byte[] buffer;

            using (MemoryStream ms= new MemoryStream())
            {
                PdfWriter.GetInstance(doc, ms);
                doc.Open();
                //titulo
                Paragraph title = new Paragraph("Listado de Usuarios");
                title.Alignment = Element.ALIGN_CENTER;
                doc.Add(title);
                Paragraph espaciador = new Paragraph(" ");
                doc.Add(espaciador);
                //columnas de tabla 
                PdfPTable table = new PdfPTable(4);
                // definir anchos de las columanas 
                float[] values = new float[4] {30,50,80,50};
                // asignar anchos de las columanas 
                table.SetWidths(values);
                //encabezados de la tabla
                PdfPCell celda1 = new PdfPCell(new Phrase("Id Usuario"));
                celda1.BackgroundColor = new BaseColor(130,130,130);
                celda1.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                table.AddCell(celda1);
                PdfPCell celda2 = new PdfPCell(new Phrase("Nombre Usuario"));
                celda2.BackgroundColor = new BaseColor(130, 130, 130);
                celda2.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                table.AddCell(celda2);
                PdfPCell celda3 = new PdfPCell(new Phrase("Email"));
                celda3.BackgroundColor = new BaseColor(130, 130, 130);
                celda3.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                table.AddCell(celda3);
                PdfPCell celda4 = new PdfPCell(new Phrase("Grupo"));
                celda4.BackgroundColor = new BaseColor(130, 130, 130);
                celda4.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                table.AddCell(celda4);
                List<UsuarioCLS> lista = (List<UsuarioCLS>)Session["listaUsuario"];
                int nregistros = lista.Count();
                for (int i = 0; i < nregistros; i++)
                {
                    table.AddCell(lista[i].idUsuario.ToString());
                    table.AddCell(lista[i].nombreGrupo);
                    table.AddCell(lista[i].email);
                    table.AddCell(lista[i].nombreGrupo);
                }
                // asignar la tabla al documento 
                doc.Add(table);
                doc.Close();

                buffer = ms.ToArray();
            }
            return File(buffer, "application/pdf");
        }

        public ActionResult Index(int pagina = 1)
        {
            listarComboGrupo();
            List<UsuarioCLS> listaUsuario = new List<UsuarioCLS>();

            PaginadorGenerico<UsuarioCLS> _PaginadorUsuarios;
            int _RegistrosPorPagina = 4;
            int _TotalRegistros = 0;

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
                                }).OrderBy(p => p.nombreUsuario)
                                .Skip((pagina - 1) * _RegistrosPorPagina)
                                .Take(_RegistrosPorPagina)
                                .ToList();

                _TotalRegistros = bd.Usuario.Where(p=>p.HABILITADO==1).Count();

                // Número total de páginas de la tabla Customers
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorUsuarios = new PaginadorGenerico<UsuarioCLS>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = listaUsuario
                };
                // sesion con listado de usuario para los archivos pdf y excel
                string nomUsuario = "";
                Session["listaUsuario"] = listaParaArchivos(nomUsuario);
            }
            return View(_PaginadorUsuarios);
        }


        public ActionResult Filtrar(string nombreusuario, int pagina = 1)
        {
            PaginadorGenerico<UsuarioCLS> _PaginadorUsuarios;
            int _RegistrosPorPagina = 4;
            int _TotalRegistros = 0;

            string nomUsuario = nombreusuario;
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
                                    }).OrderBy(p => p.nombreUsuario)
                                    .Skip((pagina - 1) * _RegistrosPorPagina)
                                    .Take(_RegistrosPorPagina)
                                    .ToList();

                    _TotalRegistros = bd.Usuario.Where(p => p.HABILITADO == 1).Count();// Número total de páginas de la tabla ususarios
                    var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                    // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                    _PaginadorUsuarios = new PaginadorGenerico<UsuarioCLS>()
                    {
                        RegistrosPorPagina = _RegistrosPorPagina,
                        TotalRegistros = _TotalRegistros,
                        TotalPaginas = _TotalPaginas,
                        PaginaActual = pagina,
                        Resultado = listaUsuario
                    };
                }
                else
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
                                    }).OrderBy(p => p.nombreUsuario)
                                    .Skip((pagina - 1) * _RegistrosPorPagina)
                                    .Take(_RegistrosPorPagina)
                                    .ToList();
                    // Número total de páginas de la tabla Customers
                    _TotalRegistros = bd.Usuario.Where(p => p.HABILITADO == 1
                                      && p.NOMBREUSUARIO.Contains(nomUsuario)).Count();              
                    var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                    // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                    _PaginadorUsuarios = new PaginadorGenerico<UsuarioCLS>()
                    {
                        RegistrosPorPagina = _RegistrosPorPagina,
                        TotalRegistros = _TotalRegistros,
                        TotalPaginas = _TotalPaginas,
                        PaginaActual = pagina,
                        Resultado = listaUsuario
                    };
                }
                // sesion con listado de usuario para los archivos pdf y excel
                Session["listaUsuario"] = listaParaArchivos(nomUsuario);
            }
            return PartialView("_TablaUsuario", _PaginadorUsuarios);
        }

        public List<UsuarioCLS> listaParaArchivos(string filtro)
        {
            List<UsuarioCLS> lista = new List<UsuarioCLS>();
            if (filtro == null)
            {
                using (var bd = new BDDemoLoginEntities())
                {
                    lista = (from usuario in bd.Usuario
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
            }else
            {
                using (var bd = new BDDemoLoginEntities())
                {
                    lista = (from usuario in bd.Usuario
                             join grupo in bd.Grupo
                             on usuario.IDGRUPO equals grupo.IDGRUPO
                             where usuario.HABILITADO == 1
                             && usuario.NOMBREUSUARIO.Contains(filtro)
                             select new UsuarioCLS
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


/*
        // GET: Usuario 
        public ActionResult IndexSinPaginado()
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
                Session["listaUsuario"] = listaUsuario;
            }
            return View(listaUsuario);
        }

*/

/*
        public ActionResult FiltrarSinPAginado(UsuarioCLS oUsuarioCLS)
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
                    Session["listaUsuario"] = listaUsuario;
                }
                else
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
                    Session["listaUsuario"] = listaUsuario;
                }
            }
            return PartialView("_TablaUsuario", listaUsuario);
        }
*/


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

/*
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
*/
/*
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

*/
/*
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

*/
        public string Eliminar(int txtIdUsuario)
        {
            string respuesta = "";
            try
            {
                using (var bd = new BDDemoLoginEntities())
                {
                    // borrado logico del usuario
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