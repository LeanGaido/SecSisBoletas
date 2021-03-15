using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.ViewModels
{
    public class VmBoletaAportes
    {
        public string IdDeclaracionJurada { get; set; }
        public string RazonSocial { get; set; }
        public string Cuit { get; set; }
        public string Mes { get; set; }
        public string Anio { get; set; }
        public string CantEmpleados { get; set; }
        public string TotalSueldos { get; set; }
        public string DosPorc { get; set; }
        public string CantAfiliados { get; set; }
        public string TotalSueldosAfiliados { get; set; }
        public string CincoPorc { get; set; }
        public string CantFamiliaresACargo { get; set; }
        public string UnPorcFamiliaresACargo { get; set; }
        public string RecargoPorMora { get; set; }
        public string TotalADepositar { get; set; }
        public string TotalDepositado { get; set; }
        public string FechaPago { get; set; }
    }
}
