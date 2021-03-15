using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [DataContract(IsReference = true)]
    [Serializable]
    [Table("tblBoletaAportes")]
    public class BoletaAportes
    {
        [Key]
        public int IdBoleta { get; set; }

        [ForeignKey("DeclaracionJurada")]
        public int IdDeclaracionJurada { get; set; }

        [Required]
        public int MesBoleta { get; set; }

        [Required]
        public int AnioBoleta { get; set; }

        [Required, Display(Name = "Fecha de Vencimiento"), DataType(DataType.DateTime)]
        
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaVencimiento { get; set; }

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

        [DataType(DataType.Currency)]
        public decimal TotalPagado { get; set; }


        [DataType(DataType.Currency)]
        public decimal? RecargoMora { get; set; }

        [Required]
        public bool BoletaPagada { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FechaPago { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FechaBoleta { get; set; }

        [NotMapped]
        [DataType(DataType.Currency)]
        public decimal TotalDepositado { get; set; }

        [NotMapped]
        [DataType(DataType.Currency)]
        public decimal TotalDepositado2 { get; set; }

        [NotMapped]
        [DataType(DataType.Currency)]
        public decimal TotalDepositado5 { get; set; }

        public bool DeBaja { get; set; }

        public DateTime? FechaBaja { get; set; }

        public string UserId { get; set; }

        public virtual DeclaracionJurada DeclaracionJurada { get; set; }
    }
}
