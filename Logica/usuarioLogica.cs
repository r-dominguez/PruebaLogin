using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using Persistencia;
using Persistencia.MODELS;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Security.Cryptography;
using System.Web.Mvc;

namespace Logica
{
    public class UsuarioLogica
    {
        public PaginadorGenericoL<Usuario_CLS> listaCompletaPaginada(int pagina)
        {
            int _RegistrosPorPagina = 4;

            int _TotalRegistros = 0;

            UsuarioP usuarioP = new UsuarioP();
            string filtro = "";
            _TotalRegistros = usuarioP.total_usuariosFiltrados(filtro);

            // Número total de páginas de la tabla usuarios
            var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);

            PaginadorGenericoL<Usuario_CLS> lista = new PaginadorGenericoL<Usuario_CLS>();

            lista = usuarioP.listaUsuarios(pagina, _RegistrosPorPagina, _TotalRegistros, _TotalPaginas);

            return lista;
        }

        public List<Usuario_CLS> listadoParaArchivos(string filtro)
        {
            List<Usuario_CLS> lista = new List<Usuario_CLS>();
            UsuarioP usuarioP = new UsuarioP();

            lista = usuarioP.usuarioParaArchivos(filtro);

            return lista;
        }

        public byte[] datosArchivoExcel(List<Usuario_CLS> usuarios)
        {
            // recuperar la lista desde la session
            List<Usuario_CLS> lista = usuarios;
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
                using (var range = ew.Cells[1, 1, 1, 4])
                {
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Font.Color.SetColor(Color.White);
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkRed);
                }
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
            return buffer;
        }


        public byte[] datosArchivoPDF(List<Usuario_CLS> usuarios)
        {
            List<Usuario_CLS> lista = usuarios;
            Document doc = new Document();
            byte[] buffer;

            using (MemoryStream ms = new MemoryStream())
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
                float[] values = new float[4] { 30, 50, 80, 50 };
                // asignar anchos de las columanas 
                table.SetWidths(values);
                //encabezados de la tabla
                PdfPCell celda1 = new PdfPCell(new Phrase("Id Usuario"));
                celda1.BackgroundColor = new BaseColor(130, 130, 130);
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
            return buffer;
        }

        public PaginadorGenericoL<Usuario_CLS> listaFiltradaPaginada(string nombreusuario, int pagina = 1)
        {
            PaginadorGenericoL<Usuario_CLS> lista = new PaginadorGenericoL<Usuario_CLS>();

            int _RegistrosPorPagina = 4;

            int _TotalRegistros = 0;

            UsuarioP usuarioP = new UsuarioP();

            string nomUsuario = nombreusuario;
            _TotalRegistros = usuarioP.total_usuariosFiltrados(nombreusuario);

            if (nomUsuario == "")
            {
                // Número total de páginas de la tabla usuarios
                int _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                lista = usuarioP.listaUsuarios(pagina, _RegistrosPorPagina, _TotalRegistros, _TotalPaginas);
            }
            else
            {
                // Número total de páginas de la tabla usuarios
                int _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                lista = usuarioP.listaUsuariosFiltrados(pagina, _RegistrosPorPagina, _TotalRegistros, _TotalPaginas, nomUsuario);
            }
            return lista;
        }

        public string eliminar(int id){

            string respuesta = "";

            UsuarioP usuarioP = new UsuarioP();

            respuesta = usuarioP.eliminar(id);

            return respuesta; 
        }

        public Usuario_CLS cambioContraGet(int id)
        {
            UsuarioP usuarioP = new UsuarioP();

            Usuario_CLS oUsuarioCLS = usuarioP.cambiarContraGet(id);

            return oUsuarioCLS;
        }

        public int cambioContraPost(Usuario_CLS oUsuario_CLS)
        {
            int respuesta = 0;
            SHA256Managed shaN = new SHA256Managed();
            byte[] byteContraN = Encoding.Default.GetBytes(oUsuario_CLS.nuevaContra);
            byte[] byteContraCifradaN = shaN.ComputeHash(byteContraN);
            string cadenaContraCifradaN = BitConverter.ToString(byteContraCifradaN).Replace("-", "");
            UsuarioP usuarioP = new UsuarioP();
            respuesta = usuarioP.cambioContraPost(oUsuario_CLS.idUsuario, cadenaContraCifradaN);
            return respuesta;
            
        }

        public Usuario_CLS recuperarUsuario(int id)
        {
            Usuario_CLS oUsuario_CLS = new Usuario_CLS();

            UsuarioP usuarioP = new UsuarioP();

            oUsuario_CLS = usuarioP.recuperarUsuario(id);

            return oUsuario_CLS;
        }

        public Usuario_CLS recuperarUsuarioNomContra(string nom, string contra)
        {
            Usuario_CLS oUsuario_CLS = new Usuario_CLS();

            UsuarioP usuarioP = new UsuarioP();

            oUsuario_CLS = usuarioP.recuperarUsuarioNomContra( nom, contra);

            return oUsuario_CLS;
        }

        

        public int RegistrosEncontradosContra(int id, string contra)
        {
            UsuarioP usuarioP = new UsuarioP();
            
            int respuesta = usuarioP.RegistrosEncontradosContra(id, contra);

            return respuesta;
        }


        public int RegistrosEncontradosUs(string nomUsuario)
        {
            int respuesta = 0;

            UsuarioP usuariop = new UsuarioP();

            respuesta = usuariop.RegistrosEncontradosUs(nomUsuario);

            return respuesta;
        }

        public int RegistrosEncontradosUsId(string nomUsuario, int id)
        {
            int respuesta = 0;

            UsuarioP usuariop = new UsuarioP();

            respuesta = usuariop.RegistrosEncontradosUsId(nomUsuario, id);

            return respuesta;
        }

        public int RegistrosEncontradosEm(string emailUsuario)
        {
            int respuesta = 0;

            UsuarioP usuariop = new UsuarioP();

            respuesta = usuariop.RegistrosEncontradosEm(emailUsuario);

            return respuesta;
        }

        public int RegistrosEncontradosEmId(string emailUsuario, int id)
        {
            int respuesta = 0;

            UsuarioP usuariop = new UsuarioP();

            respuesta = usuariop.RegistrosEncontradosEmId(emailUsuario, id);

            return respuesta;
        }

        public string guardar(Usuario_CLS oUsuario_CLS, int id)
        {
            UsuarioP usuarioP = new UsuarioP();
            string cadenaContraCifrada = "";

            if (id== -1)
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] byteContra = Encoding.Default.GetBytes(oUsuario_CLS.contra);
                byte[] byteContraCifrada = sha.ComputeHash(byteContra);
                cadenaContraCifrada = BitConverter.ToString(byteContraCifrada).Replace("-", "");
            }

            string respuesta = usuarioP.guardar(oUsuario_CLS, id, cadenaContraCifrada);
            
            return respuesta;
        }

        public int RegistrosEncontradosNomContra(string nom, string contra)
        {
            UsuarioP usuarioP = new UsuarioP();

            int respuesta = usuarioP.RegistrosEncontradosNomContra(nom, contra);

            return respuesta;
        }
        
        public string recuperarContra(string correo, string rutaLog)
        {
            UsuarioP usuarioP = new UsuarioP();
            string nombre = "";
            string respuesta = "";

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

            nombre = usuarioP.actualizarContra(correo, cadenContraCifrada);

            string asunto = "Prueba Login - Recuperación de contraseña";
            string contenido = " <h1>Recuperación de Contraseña</h1><p> Nombre de usuario:" + nombre + " </p><p> contraseña: " + nuevaContra + " </p><p> Cordial Saludo! </p> ";

            if (nombre != "") 
            {
                CORREO.enviarCorreo(correo, asunto, contenido, rutaLog);
                respuesta = "1";
            }

            return respuesta;
        }
    }
}