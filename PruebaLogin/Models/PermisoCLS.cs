using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PruebaLogin.Models
{
    public class PermisoCLS
    {
        [Display(Name = "Id Permiso")]
        public int idPermiso { get; set; }

        [Display(Name = "Nombre Página")]
        [Required]
        [StringLength(100, ErrorMessage = "La longitud máxima es de 100")]
        public string nombrePagina { get; set; }

        [Display(Name = "Nombre Acción")]
        [Required]
        [StringLength(100, ErrorMessage = "La longitud máxima es de 100")]
        public string nombreAccion { get; set; }

        [Display(Name = "Nombre Controlador")]
        [Required]
        [StringLength(100, ErrorMessage = "La longitud máxima es de 100")]
        public string nombreControlador { get; set; }

        public int habilitado { get; set; }
    }
}