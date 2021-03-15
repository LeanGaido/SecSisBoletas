using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [DataContract(IsReference = true)]
    [Table("tblJornadas")]
    public class Jornada
    {
        [Key]
        public int IdJornada { get; set; }

        [Required, Display(Name = "Jornada")]
        [StringLength(maximumLength: 30, ErrorMessage = "El Nombre de la jornada no puede superar los 30 caracteres")]
        public string Descripcion { get; set; }

        public virtual ICollection<EmpleadoEmpresa> Empleados { get; set; }
    }
}
