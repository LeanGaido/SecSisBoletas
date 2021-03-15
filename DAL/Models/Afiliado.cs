using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblAfiliados")]
    public class Afiliado
    {
        [Key]
        public int IdAfiliado { get; set; }

        [Required, ForeignKey("EmpleadoEmpresa")]
        public int IdEmpleadoEmpresa { get; set; }

        [Required]
        [Display(Name = "Cant. de familiares a cargo")]
        public int CantFamiliaresACargo { get; set; }

        [Required(ErrorMessage = "La fecha es requerida"), Display(Name = "Fecha Alta"), DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaAlta { get; set; }

        [Display(Name = "Fecha Baja"), DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FechaBaja { get; set; }

        [NotMapped]
        public bool ReActivate { get; set; }

        public virtual EmpleadoEmpresa EmpleadoEmpresa { get; set; }
    }
}
