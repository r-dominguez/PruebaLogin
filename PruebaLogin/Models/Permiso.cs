//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PruebaLogin.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Permiso
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Permiso()
        {
            this.GrupoPermiso = new HashSet<GrupoPermiso>();
        }
    
        public int IDPERMISO { get; set; }
        public string NOMBREPAGINA { get; set; }
        public string NOMBREACCION { get; set; }
        public string NOMBRECONTROLADOR { get; set; }
        public Nullable<int> HABILITADO { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GrupoPermiso> GrupoPermiso { get; set; }
    }
}
