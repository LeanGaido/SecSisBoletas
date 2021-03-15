using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblBoletasAportesEspeciales")]
    public class BoletaAportesEspecial
    {
        [Key]
        public int IdBoleta { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FechaBoleta { get; set; }

        public string Observaciones { get; set; }


        public bool BoletaPagada { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FechaPago { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaVencimiento { get; set; }

        #region Empresa
        public int IdEmpresa { get; set; }

        [Display(Name = "CUIT")]
        public string Cuit { get; set; }

        [Display(Name = "Razon Social")]
        public string RazonSocial { get; set; }

        [Display(Name = "Nombre de Fantasia")]
        public string NombreFantasia { get; set; }

        public string Calle { get; set; }

        public int Altura { get; set; }

        public string Localidad { get; set; }

        public string CodPostal { get; set; }

        public string TelefonoFijo { get; set; }

        public string TelefonoCelular { get; set; }
        #endregion

        #region Periodo
        [Required]
        public string Periodo { get; set; }
        //[Required]
        //public int MesDesdeBoleta { get; set; }

        //[Required]
        //public int AnioDesdeBoleta { get; set; }

        //[Required]
        //public int MesHastaBoleta { get; set; }

        //[Required]
        //public int AnioHastaBoleta { get; set; }
        #endregion

        #region Empleados y Sueldos
        [Required]
        public int CantEmpleados { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal TotalSueldos2 { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Aportes2 { get; set; }
        #endregion

        #region Afiliados y Sueldos
        [Required]
        public int CantAfiliados { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal TotalSueldos5 { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Aportes5 { get; set; }
        #endregion

        #region Mora y Totales
        [Required]
        [DataType(DataType.Currency)]
        public decimal? RecargoMora { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Total { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal TotalDepositado { get; set; }
        #endregion

        public bool DeBaja { get; set; }

        public DateTime? FechaBaja { get; set; }

        public string UserId { get; set; }
    }
}
