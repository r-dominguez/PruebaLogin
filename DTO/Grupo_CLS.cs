using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Web;


namespace DTO
{
    public class Grupo_CLS
    {
        [Display(Name = "Id Grupo")]
        public int idGrupo { get; set; }

        [Display(Name = "Nombre Grupo")]
        [Required]
        [StringLength(100, ErrorMessage = "La longitud máxima es de 100")]
        public string nombreGrupo { get; set; }

        public int habilitado { get; set; }

        // atributo adicional 

        public string mensajeErrorNombre { get; set; }
        public string mensajeErrorPermiso { get; set; }

        public int cantPermisos { get; set; }
    }
}