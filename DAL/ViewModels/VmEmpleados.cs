namespace DAL.ViewModels
{
    public class VmEmpleados
    {
        public int IdEmpleado { get; set; }
        public string NombreEmpleado { get; set; }
        public string ApellidoEmpleado { get; set; }
        public string NombreEmpresa { get; set; }
        public string CuilEmpleado { get; set; }
        public string CuitEmpresa { get; set; }
        public string CalleEmpleado { get; set; }
        public string AlturaEmpleado { get; set; }
        public string LocalidadEmpleado { get; set; }
        public string ProvinciaEmpleado { get; set; }
        public string CategoríaEmpleado { get; set; }
        public string JornadaEmpleado { get; set; }
        public string FechaAltaEmpleado { get; set; }
        public string FechaBajaEmpleado { get; set; }
        public string Licencia_Liquidacion { get; set; }
        public decimal Sueldo { get; set; }
        public bool EsAfiliado { get; set; }
    }
}