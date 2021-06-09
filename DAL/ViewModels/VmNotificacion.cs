using DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DAL.ViewModels
{
    public class VmNotificacion
    {
        [Required]
        public int idNotificacion { get; set; }

        [Required]
        public int idEmpresa { get; set; }

        public string RazonSocial { get; set; }

        public DateTime Fecha { get; set; }

        [Required]
        [StringLength(maximumLength: 80, ErrorMessage = "El Titulo no puede superar los 80 caracteres")]
        public string Titulo { get; set; }

        [Required]
        public string Descripcion { get; set; }

        public bool Visto { get; set; }

        public DateTime? FechaVisto { get; set; }

        public string UserId { get; set; }

        public List<AdjuntoNotificacion> ListadoAdjuntos { get; set; }

        public List<HttpPostedFileBase> Adjuntos { get; set; }
    }
}
