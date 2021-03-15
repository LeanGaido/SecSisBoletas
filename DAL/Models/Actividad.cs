using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblActividades")]
    public class Actividad
    {
        [Key]
        public int IdActividad { get; set; }

        [Required]
        [StringLength(maximumLength: 255, ErrorMessage = "El nombre no puede superar los 30 caracteres")]
        public string Nombre { get; set; }
    }
}
