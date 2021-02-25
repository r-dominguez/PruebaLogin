using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class GrupoPermiso_CLS
    {
        [Display(Name = "Id Grupo Permiso")]
        public int idGrupoPermiso { get; set; }

        [Display(Name = "Id Grupo")]
        [Required]
        public int idGrupo { get; set; }

        [Display(Name = "Id Permiso")]
        [Required]
        public int idPermiso { get; set; }

        public int habilitado { get; set; }

        // atributos adicionales 

        [Display(Name = "Nombre Grupo")]
        public string nombreGrupo { get; set; }

        [Display(Name = "Nombre Pagina")]
        public string nombrePagina { get; set; }
    }
}
