using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblLicenciasEmpleados")]
    public class LicenciaEmpleado
    {
        [Key]
        public int IdLicenciaEmpleado { get; set; }

        [Required, ForeignKey("LicenciaLaboral")]
        public int IdLicenciaLaboral { get; set; }

        [Required, ForeignKey("EmpleadoEmpresa")]
        public int IdEmpleadoEmpresa { get; set; }

        [Required]
        [Display(Name = "Fecha de Alta"), DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaAltaLicencia { get; set; }
        
        [Display(Name = "Fecha de Baja"), DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FechaBajaLicencia { get; set; }


        public virtual LicenciaLaboral LicenciaLaboral { get; set; }
        public virtual EmpleadoEmpresa EmpleadoEmpresa { get; set; }
    }
}
