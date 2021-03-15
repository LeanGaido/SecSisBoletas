using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblDetallesDeclaracionJurada")]
    public class DetalleDeclaracionJurada
    {
        [Key]
        public int IdDetalleDeclaracionJurada { get; set; }

        [Required, ForeignKey("DeclaracionJurada")]
        public int IdDeclaracionJurada { get; set; }

        [Required, ForeignKey("EmpleadoEmpresa")]
        public int IdEmpleadoEmpresa { get; set; }

        [NotMapped]
        public bool EsAfiliado { get; set; }

        [Required, ForeignKey("Categoria")]
        public int idCategoria { get; set; }

        [Required, ForeignKey("Jornada")]
        public int idJornadaLaboral { get; set; }

        [NotMapped]
        public bool LicenciaEmpleado { get; set; }

        [NotMapped]
        public bool LiquidacionProporcional { get; set; }

        [NotMapped]
        public int? IdLiquidacionProporcional { get; set; }

        //Sueldo 2%
        [Required, DataType(DataType.Currency)]
        public decimal Sueldo { get; set; }

        //Sueldo 5%
        [Required, DataType(DataType.Currency)]
        public decimal SueldoBase { get; set; }

        public virtual DeclaracionJurada DeclaracionJurada { get; set; }
        public virtual EmpleadoEmpresa EmpleadoEmpresa { get; set; }
        public virtual Categoria Categoria { get; set; }
        public virtual Jornada Jornada { get; set; }

        public virtual ICollection<LiquidacionProporcionalEmpleado> LiquidacionesProporcionalesEmpleados { get; set; }
    }
}
