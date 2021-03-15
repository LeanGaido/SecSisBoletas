using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.ViewModels
{
    public class VmUsuario
    {
        public string Id { get; set; }

        public int IdEmpresa { get; set; }

        public string RazonSocial { get; set; }

        public string NombreFantasia { get; set; }

        public string Email { get; set; }

        public string Cuit { get; set; }

        public string Telefono { get; set; }

        public string Celular { get; set; }

        public int Page { get; set; }

        public string CurrentFilter { get; set; }
    }
}
