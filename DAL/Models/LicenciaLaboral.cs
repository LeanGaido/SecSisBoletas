using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblLicenciasLaborales")]
    public class LicenciaLaboral
    {
        [Key]
        public int IdLicenciaLaboral { get; set; }

        [Required, Display(Name = "Licencia")]
        [StringLength(maximumLength: 30, ErrorMessage = "El Nombre de la licencia no puede superar los 30 caracteres")]
        public string Descripcion { get; set; }
    }
}
