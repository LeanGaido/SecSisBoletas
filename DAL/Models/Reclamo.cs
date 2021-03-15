using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblReclamos")]
    public class Reclamo
    {
        [Key]
        public int IdReclamo { get; set; }

        [Required, StringLength(50)]
        public string Nombre { get; set; }

        [Required, StringLength(50)]
        public string Empresa { get; set; }

        [Required, StringLength(50), DataType(DataType.PhoneNumber)]
        public string Telefono { get; set; }

        [Required, StringLength(50), DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required, Display(Name = "Reclamo"), StringLength(256)]
        public string Mensaje { get; set; }
    }
}
