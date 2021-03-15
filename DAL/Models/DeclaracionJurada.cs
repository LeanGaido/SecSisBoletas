using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblDeclaracionesJuradas")]
    public class DeclaracionJurada
    {
        [Key]
        public int IdDeclaracionJurada { get; set; }

        [Required, ForeignKey("Empresa")]
        public int idEmpresa { get; set; }

        [Required]
        public int mes { get; set; }

        [NotMapped]
        public string NombreMes { get; set; }

        [Required]
        public int anio { get; set; }

        [NotMapped]
        public string MesAnio { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? fecha { get; set; }

        public bool DeBaja { get; set; }

        public DateTime? FechaBaja { get; set; }

        public string UserId { get; set; }

        public virtual Empresa Empresa { get; set; }
        public virtual IEnumerable<DetalleDeclaracionJurada> DetallesDeclaracionJurada { get; set; }
    }
}
