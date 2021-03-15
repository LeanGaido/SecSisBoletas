using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblTurnosEmpleados")]
    public class TurnoEmpleado
    {
        [Key]
        public int IdTurnoEmpleado { get; set; }

        [ForeignKey("EmpleadoEmpresa")]
        public int IdEmpleadoEmpresa { get; set; }

        [Required, StringLength(50)]
        public string Turno { get; set; }

        public virtual EmpleadoEmpresa EmpleadoEmpresa { get; set; }
    }
}
