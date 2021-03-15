using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.ViewModels
{
    public class VmEmpleadosEmpresas
    {
        public string RazonSocialEmpresa { get; set; }
        public string CuitEmpresa { get; set; }
        public string CalleEmpresa { get; set; }
        public string LocalidadEmpresa { get; set; }
        public string ProvinciaEmpresa { get; set; }

        public List<VmEmpleados> Empleados { get; set; }
    }
}
