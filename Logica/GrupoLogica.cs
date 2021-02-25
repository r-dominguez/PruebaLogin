using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using DTO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Persistencia;

namespace Logica
{
    public class GrupoLogica
    {
        public List<SelectListItem> listaComboGrupo()
        {
            List<SelectListItem> lista = new List<SelectListItem>();
            GrupoP grupop = new GrupoP();
            lista = grupop.listaComboGrupos();
            return lista;
        }

        public PaginadorGenericoL<Grupo_CLS> listaCompletaPaginada(int pagina)
        {
            int _RegistrosPorPagina = 4;

            int _TotalRegistros = 0;

            GrupoP grupoP = new GrupoP();

            string filtro = "";
            _TotalRegistros = grupoP.total_gruposFiltrado(filtro);

            // Número total de páginas de la tabla usuarios
            var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);

            PaginadorGenericoL<Grupo_CLS> lista = new PaginadorGenericoL<Grupo_CLS>();

            lista = grupoP.listaGrupos(pagina, _RegistrosPorPagina, _TotalRegistros, _TotalPaginas);

            return lista;
        }

        public List<Grupo_CLS> listadoParaArchivos(string filtro)
        {
            List<Grupo_CLS> lista = new List<Grupo_CLS>();
            GrupoP grupoP = new GrupoP();

            lista = grupoP.gruposParaArchivos(filtro);

            return lista;
        }


        public byte[] datosArchivoExcel(List<Grupo_CLS> grupos)
        {
            // recuperar la lista desde la session
            List<Grupo_CLS> lista = grupos;
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
                ew.Cells[1, 3].Value = "Cant Permisos";
                ew.Column(1).Width = 10;
                ew.Column(2).Width = 30;
                ew.Column(3).Width = 10;
                using (var range = ew.Cells[1, 1, 1, 3])
                {
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Font.Color.SetColor(Color.White);
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkRed);
                }
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
            return buffer;
        }

        public int cantPermisos(int id)
        {
            GrupoP grupoP = new GrupoP();

            int nPermisos = grupoP.cantPermisos(id);

            return nPermisos;
        }


        public PaginadorGenericoL<Grupo_CLS> listaFiltradaPaginada(string nombregrupo, int pagina = 1)
        {
            PaginadorGenericoL<Grupo_CLS> lista = new PaginadorGenericoL<Grupo_CLS>();

            int _RegistrosPorPagina = 4;

            int _TotalRegistros = 0;

            GrupoP grupoP = new GrupoP();

            string nomGrupo = nombregrupo;

            _TotalRegistros = grupoP.total_gruposFiltrado(nomGrupo);

            if (nomGrupo == null)
            {
                // Número total de páginas de la tabla usuarios
                int _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                lista = grupoP.listaGrupos(pagina, _RegistrosPorPagina, _TotalRegistros, _TotalPaginas);
            }
            else
            {
                // Número total de páginas de la tabla usuarios
                int _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                lista = grupoP.listaGruposFiltrados(pagina, _RegistrosPorPagina, _TotalRegistros, _TotalPaginas, nomGrupo);
            }
            return lista;
        }

        public List<Grupo_CLS> listarGruposActivos()
        {
            List<Grupo_CLS> lista = new List<Grupo_CLS>();

            GrupoP grupoP = new GrupoP();
            lista = grupoP.listarGruposActivos();

            return lista;
        }

        public List<Grupo_CLS> gruposAgraficar(int[] grupoParaGraficar)
        {
            int[] arregloGrupoParaGraficar = grupoParaGraficar;

            List<Grupo_CLS> lista = new List<Grupo_CLS>();
            List<Grupo_CLS> listaEnBase = new List<Grupo_CLS>();
            GrupoP grupoP = new GrupoP();
            listaEnBase = grupoP.listarGruposActivos();

            for (int i = 0; i < listaEnBase.Count(); i++)
            {
                listaEnBase[i].cantPermisos = cantPermisos(listaEnBase[i].idGrupo);

            }

            foreach (var item in listaEnBase)
            {
                for (int i = 0; i < arregloGrupoParaGraficar.Length; i++)
                {
                    if (item.idGrupo == arregloGrupoParaGraficar[i])
                    {
                        lista.Insert(0, item);
                    }
                }
            }
            var listaOrdenada = lista.OrderBy(p => p.nombreGrupo).ToList();
            return listaOrdenada;
        }

        public string eliminar(int id)
        {
            GrupoP grupoP = new GrupoP();
            string respuesta = grupoP.eliminar(id);

            return respuesta;
        }

        public int RegistrosEncontrados(string nomGrupo, int idGrupo)
        {
            GrupoP grupoP = new GrupoP();

            int respuesta = grupoP.RegistrosEncontrados(nomGrupo, idGrupo);

            return respuesta;
        }

        public string guardar(Grupo_CLS oGrupo_CLS, int titulo, string[] permisosSeleccionados)
        {
            string respuesta = "";
            GrupoP grupoP = new GrupoP();

            respuesta = grupoP.guardar(oGrupo_CLS, titulo, permisosSeleccionados);

            if (respuesta == "0") respuesta = "";
            else respuesta = "999";

            return respuesta;
        }

        public List<Grupo_CLS> recuperarCantPermisosGrupo()
        {
            List<Grupo_CLS> lista = new List<Grupo_CLS>();

            GrupoP grupoP = new GrupoP();

            lista = grupoP.recuperarCantPermisosGrupo();

            return lista;
        }

        public Grupo_CLS RecuperarDatosGrupo(int id)
        {
            Grupo_CLS oGrupo_CLS = new Grupo_CLS();
            
            GrupoP grupoP = new GrupoP();

            oGrupo_CLS = grupoP.RecuperarDatosGrupo(id);

            return oGrupo_CLS;
        }


        public List<SelectListItem> RecuperarPermisoDisponibles()
        {
            List<SelectListItem> lista = new List<SelectListItem>();
            GrupoP grupoP = new GrupoP();

            lista = grupoP.RecuperarPermisoDisponibles();

            return lista;
        }


        public List<SelectListItem> RecuperarDatosGrupoPermiso(int id)
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            GrupoP grupoP = new GrupoP();

            lista = grupoP.RecuperarDatosGrupoPermiso(id);

            return lista;
        }

        public List<SelectListItem> RecuperarDatosGrupoPermisoSinAsignar(int titulo)
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            GrupoP grupoP = new GrupoP();

            List<Permiso_CLS> listaP = new List<Permiso_CLS>();
            
            listaP = grupoP.RecuperarPermisoEnBase();

            List<GrupoPermiso_CLS> listagp = new List<GrupoPermiso_CLS>();

            listagp = grupoP.RecuperarPermisosDeGrupo(titulo);

            foreach (Permiso_CLS p in listaP)
            {
                int cantSi = 0;
                foreach (GrupoPermiso_CLS gp in listagp)
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
            return lista;
        }
     }
}
