using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.ViewModels
{
    public class VmEmpresa
    {
        public string Cuit { get; set; }
        
        public string RazonSocial { get; set; }
        
        public string NombreFantasia { get; set; }
        
        public string Calle { get; set; }
        
        public string Altura { get; set; }
        
        public string Localidad { get; set; }

        public string Provincia { get; set; }

        public string TelefonoFijo { get; set; }

        public string TelefonoCelular { get; set; }
        
        public string Email { get; set; }
        
        public string Actividad { get; set; }
        
        public string FechaAltaEmpresa { get; set; }
        
        public string FechaBajaEmpresa { get; set; }
    }
}
