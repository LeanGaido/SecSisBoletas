using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.ViewModels
{
    public class VmPreviewNotificacion
    {
        public int ID { get; set; }

        public int EmpresaId { get; set; }

        public DateTime FechaAux { get; set; }

        public string Fecha { get; set; }

        public string Titulo { get; set; }
    }
}
