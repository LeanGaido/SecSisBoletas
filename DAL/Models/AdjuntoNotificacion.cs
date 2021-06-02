using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblAdjuntosNotificacion")]
    public class AdjuntoNotificacion
    {
        [Key]
        public int IdAdjuntoNotificacion { get; set; }

        [Required, ForeignKey("Notificacion")]
        public int idNotificacion { get; set; }

        public string Adjunto { get; set; }

        public virtual Notificacion Notificacion { get; set; }
    }
}
