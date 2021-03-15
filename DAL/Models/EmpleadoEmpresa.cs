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
    [Serializable]
    [Table("tblEmpleadosEmpresas")]
    public class EmpleadoEmpresa
    {
        [Key]
        public int idEmpleadoEmpresa { get; set; }

        [Required, ForeignKey("Empleado")]
        public int idEmpleado { get; set; }

        [NotMapped]
        public string NombreEmpleado { get; set; }

        [Required, ForeignKey("Empresa")]
        public int idEmpresa { get; set; }

        [Required]
        [ForeignKey("Categoria")]
        public int IdCategoria { get; set; }

        [Required]
        [ForeignKey("Jornada")]
        public int IdJornada { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaAlta { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FechaBaja { get; set; }

        public bool EsAfiliado { get; set; }

        public virtual Empleado Empleado { get; set; }
        public virtual Empresa Empresa { get; set; }
        public virtual Categoria Categoria { get; set; }
        public virtual Jornada Jornada { get; set; }
    }
}
