using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblNotificaciones")]
    public class Notificacion
    {
        [Key]
        public int IdNotificacion { get; set; }

        [Required, ForeignKey("Empresa")]
        public int idEmpresa { get; set; }

        public DateTime Fecha { get; set; }

        [Required]
        [StringLength(maximumLength: 80, ErrorMessage = "El Titulo no puede superar los 80 caracteres")]
        public string Titulo { get; set; }

        [Required]
        [StringLength(maximumLength: 255, ErrorMessage = "La Descripcion no puede superar los 255 caracteres")]
        public string Descripcion { get; set; }

        public bool Visto { get; set; }

        public DateTime FechaVisto { get; set; }

        public virtual Empresa Empresa { get; set; }
    }
}
