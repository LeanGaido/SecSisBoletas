using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class AsesorContable
    {
        public int ID { get; set; }

        [Required, ForeignKey("Empresa")]
        public int idEmpresa { get; set; }

        public string NombreContacto { get; set; }

        public string AreaTelefono { get; set; }

        public string NumeroTelefono { get; set; }
        
        public string AreaCelular { get; set; }

        public string NumeroCelular { get; set; }

        public string Email { get; set; }

        public virtual Empresa Empresa { get; set; }
    }
}
