using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblLiquidacionesProporcionalesEmpleado")]
    public class LiquidacionProporcionalEmpleado
    {
        [Key]
        public int IdLiquidacionProporcionalEmpleado { get; set; }

        [ForeignKey("LiquidacionProporcional")]
        public int IdLiquidacionProporcional { get; set; }

        [ForeignKey("DetalleDeclaracionJurada")]
        public int IdDetalleDeclaracionJurada { get; set; }

        public virtual LiquidacionProporcional LiquidacionProporcional { get; set; }
        public virtual DetalleDeclaracionJurada DetalleDeclaracionJurada { get; set; }

    }
}
