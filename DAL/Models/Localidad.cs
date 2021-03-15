using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblLocalidades")]
    public class Localidad
    {
        [Key]
        public int IdLocalidad { get; set; }

        [Required]
        [StringLength(maximumLength: 30, ErrorMessage = "El Nombre de la localidad no puede superar los 30 caracteres")]
        public string Nombre { get; set; }

        [NotMapped]
        public string NombreCodPostal { get; set; }

        [Required]
        [Display(Name = "Codigo Postal")]
        public int CodPostal { get; set; }

        [ForeignKey("Provincia")]
        public int IdProvincia { get; set; }

        public virtual Provincia Provincia { get; set; }
        public virtual ICollection<Empleado> Empleados { get; set; }
    }
}
