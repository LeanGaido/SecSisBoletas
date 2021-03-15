using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblHorariosEmpleados")]
    public class HorarioEmpleado
    {
        [Key]
        public int IdHorarioEmpleado { get; set; }

        [ForeignKey("EmpleadoEmpresa")]
        public int IdEmpleadoEmpresa { get; set; }

        [Required, StringLength(20)]
        public string Dia { get; set; }

        [Required, StringLength(20)]
        public string Turno { get; set; }

        [Required, StringLength(20)]
        public string Entrada { get; set; }

        [Required, StringLength(20)]
        public string Salida { get; set; }

        public virtual EmpleadoEmpresa EmpleadoEmpresa { get; set; }
    }
}
