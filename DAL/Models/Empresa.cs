using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblEmpresas")]
    public class Empresa
    {
        [Key]
        public int IdEmpresa { get; set; }

        [Required]
        [Display(Name = "CUIT")]
        public string Cuit { get; set; }

        [Required, Display(Name = "Razon Social")]
        public string RazonSocial { get; set; }

        [Required, Display(Name = "Nombre de Fantasia")]
        public string NombreFantasia { get; set; }

        [Required]
        public string Calle { get; set; }

        [Required]
        public int Altura { get; set; }

        [Required, Display(Name = "Localidad")]
        [ForeignKey("Localidad")]
        public int IdLocalidad { get; set; }

        public string TelefonoFijo { get; set; }

        public string TelefonoCelular { get; set; }

        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required, Display(Name = "Actividad")]
        [ForeignKey("Actividad")]
        public int IdActividad { get; set; }

        [Required, Display(Name = "Fecha de Alta")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaAltaEmpresa { get; set; }

        [Display(Name = "Fecha de Baja")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FechaBajaEmpresa { get; set; }

        public virtual Localidad Localidad { get; set; }
        public virtual Actividad Actividad { get; set; }


        public virtual ICollection<EmpleadoEmpresa> Empleados { get; set; }
    }
}
