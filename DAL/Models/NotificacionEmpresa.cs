using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblNotificacionesEmpresa")]
    public class NotificacionEmpresa
    {
        [Key]
        public int IdNotificacionEmpresa { get; set; }

        [Required, ForeignKey("Empresa")]
        public int idEmpresa { get; set; }

        [Required, ForeignKey("Notificacion")]
        public int IdNotificacion { get; set; }

        public bool Visto { get; set; }

        public DateTime? FechaVisto { get; set; }

        public virtual Empresa Empresa { get; set; }

        public virtual Notificacion Notificacion { get; set; }
    }
}
