using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblLiquidacionesProporcionales")]
    public class LiquidacionProporcional
    {
        [Key]
        public int IdLiquidacionProporcional { get; set; }

        [Required, Display(Name = "Liquidacion Proporcional")]
        [StringLength(maximumLength: 80, ErrorMessage = "El Nombre de la liquidacion proporcional no puede superar los 80 caracteres")]
        public string Descripcion { get; set; }

        public virtual ICollection<LiquidacionProporcionalEmpleado> LiquidacionesProporcionalesEmpleados { get; set; }
    }
}
