using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.ViewModels
{
    public class VmListadoNotificaciones
    {
        public int ID { get; set; }

        public int EmpresaId { get; set; }

        public string RazonSocial { get; set; }

        public DateTime Fecha { get; set; }

        public string Titulo { get; set; }

        public bool Visto { get; set; }

        public DateTime? FechaVisto { get; set; }
    }
}
