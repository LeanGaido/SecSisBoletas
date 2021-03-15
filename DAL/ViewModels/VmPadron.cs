using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.ViewModels
{
    public class VmPadron
    {
        public int IdEmpresa { get; set; }

        public string RazonSocial { get; set; }

        public string Cuit { get; set; }

        public string Localidad { get; set; }

        public int CantEmpleados { get; set; }
    }
}
