using DAL.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecSisBoletas.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Código")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "¿Recordar este explorador?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Cuit")]
        public string UserName { get; set; }

        //[Required]
        //[Display(Name = "Correo electrónico")]
        //[EmailAddress]
        //public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Display(Name = "¿Recordar cuenta?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "El número de caracteres de {0} debe ser al menos {2}.", MinimumLength = 6)]
        [DataType(DataType.Password, ErrorMessage = "La Contraseña tiene que contar con una Mayuscula, una Miniscula, un Numero, un caracter especial('*', '-', etc) y tener un largo de almenos 6 caracteres")]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [DataType(DataType.Password, ErrorMessage = "La Contraseña tiene que contar con una Mayuscula, una Miniscula, un Numero, un caracter especial('*', '-', etc) y tener un largo de almenos 6 caracteres")]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; }

        //Empresa
        [Required]
        [Display(Name = "CUIT")]
        public string Cuit { get; set; }

        [Required, Display(Name = "Razon Social")]
        public string RazonSocial { get; set; }

        [Required, Display(Name = "Nombre de Fantasia")]
        public string NombreFantasia { get; set; }

        [Required]
        public string Calle { get; set; }

        [Required]
        public int Altura { get; set; }

        [Required, Display(Name = "Localidad")]
        [ForeignKey("Localidad")]
        public int IdLocalidad { get; set; }

        public string TelefonoFijo { get; set; }

        public string TelefonoCelular { get; set; }

        [Required, Display(Name = "Actividad")]
        [ForeignKey("Actividad")]
        public int IdActividad { get; set; }

        public virtual Localidad Localidad { get; set; }
        public virtual Actividad Actividad { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "El número de caracteres de {0} debe ser al menos {2}.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y la contraseña de confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }
    }
}
