using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblProvincias")]
    public class Provincia
    {
        [Key]
        public int IdProvincia { get; set; }

        [StringLength(maximumLength: 30, ErrorMessage = "El Nombre de la provincia no puede superar los 30 caracteres" )]
        public string Nombre { get; set; }

        public virtual ICollection<Localidad> Localidades { get; set; }
    }
}
