using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PruebaLogin.Models
{
    public class UsuarioCLS
    {
        [Display(Name = "Id Usuario")]
        public int idUsuario { get; set; }

        [Display(Name = "Nombre Usuario")]
        [Required]
        [StringLength(100, ErrorMessage = "La longitud máxima es de 100")]
        public string nombreUsuario { get; set; }

        [Display(Name = "Contraseña")]
        [Required]
        [StringLength(100, ErrorMessage = "La longitud máxima es de 100")]
        public string contra { get; set; }

        [Display(Name = "Id Grupo")]
        [Required]
        public int idGrupo { get; set; }

        public int habilitado { get; set; }

        // atributos adicionales 

        [Display(Name = "Nombre Grupo")]
        public string nombreGrupo { get; set; }

        // atributos adicionales 

        public string mensajeError { get; set; }
    }
}