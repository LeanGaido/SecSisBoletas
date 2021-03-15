using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblFechasVencimientos")]
    public class FechaVencimiento
    {
        [Key]
        public int IdFechaVencimiento { get; set; }

        //[NotMapped, Display(Name = "Mes")]
        [Display(Name = "Mes")]
        public int mesBoleta { get; set; }

        //[NotMapped, Display(Name = "Año")]
        [Display(Name = "Año")]
        public int anioBoleta { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaVto { get; set; }
    }
}
