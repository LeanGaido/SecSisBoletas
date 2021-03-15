using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("tblEmpleados")]
    public class Empleado
    {
        [Key]
        public int IdEmpleado { get; set; }

        [NotMapped]
        public int IdEmpresa { get; set; }

        [Required]
        [StringLength(maximumLength: 30, ErrorMessage = "El nombre no puede superar los 30 caracteres")]
        public string Nombre { get; set; }

        [Required]
        [StringLength(maximumLength: 30, ErrorMessage = "El apellido no puede superar los 30 caracteres")]
        public string Apellido { get; set; }

        [Required]
        [Display(Name = "CUIL")]
        public string Cuil { get; set; }

        [Required]
        [StringLength(maximumLength: 50, ErrorMessage = "El nombre de la calle no puede superar los 50 caracteres")]
        public string Calle { get; set; }

        [Required]
        public int Altura { get; set; }

        [NotMapped]
        public int IdProvincia { get; set; }

        [Required]
        [ForeignKey("Localidad")]
        public int IdLocalidad { get; set; }

        //public int Telefono { get; set; }

        //[Required]
        //public int Celular { get; set; }

        //[Required, DataType(DataType.EmailAddress)]
        //public string Email { get; set; }

        [NotMapped]
        public int IdJornada { get; set; }

        [NotMapped]
        public string Jornada { get; set; }

        [NotMapped]
        public int IdCategoria { get; set; }

        [NotMapped]
        public string Categoria { get; set; }

        [NotMapped]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaAlta { get; set; }

        [NotMapped]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaBaja { get; set; }

        [NotMapped]
        public bool EsAfiliado { get; set; }

        [NotMapped]
        public bool ReActivate { get; set; }

        [NotMapped]
        public string Turno { get; set; }

        [NotMapped]
        public string desdeLunesManana { get; set; }

        [NotMapped]
        public string hastaLunesManana { get; set; }

        [NotMapped]
        public string desdeLunesTarde { get; set; }

        [NotMapped]
        public string hastaLunesTarde { get; set; }

        [NotMapped]
        public string desdeMartesManana { get; set; }

        [NotMapped]
        public string hastaMartesManana { get; set; }

        [NotMapped]
        public string desdeMartesTarde { get; set; }

        [NotMapped]
        public string hastaMartesTarde { get; set; }

        [NotMapped]
        public string desdeMiercolesManana { get; set; }

        [NotMapped]
        public string hastaMiercolesManana { get; set; }

        [NotMapped]
        public string desdeMiercolesTarde { get; set; }

        [NotMapped]
        public string hastaMiercolesTarde { get; set; }

        [NotMapped]
        public string desdeJuevesManana { get; set; }

        [NotMapped]
        public string hastaJuevesManana { get; set; }

        [NotMapped]
        public string desdeJuevesTarde { get; set; }

        [NotMapped]
        public string hastaJuevesTarde { get; set; }

        [NotMapped]
        public string desdeViernesManana { get; set; }

        [NotMapped]
        public string hastaViernesManana { get; set; }

        [NotMapped]
        public string desdeViernesTarde { get; set; }

        [NotMapped]
        public string hastaViernesTarde { get; set; }

        [NotMapped]
        public string desdeSabadoManana { get; set; }

        [NotMapped]
        public string hastaSabadoManana { get; set; }

        [NotMapped]
        public string desdeSabadoTarde { get; set; }

        [NotMapped]
        public string hastaSabadoTarde { get; set; }

        [NotMapped]
        public string desdeDomingoManana { get; set; }

        [NotMapped]
        public string hastaDomingoManana { get; set; }

        [NotMapped]
        public string desdeDomingoTarde { get; set; }

        [NotMapped]
        public string hastaDomingoTarde { get; set; }

        public virtual Localidad Localidad { get; set; }

        public virtual ICollection<EmpleadoEmpresa> Empresas { get; set; }
    }
}
