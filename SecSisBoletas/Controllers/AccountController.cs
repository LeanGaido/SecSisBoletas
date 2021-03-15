using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SecSisBoletas.Models;
using DAL;
using DAL.Models;
using System.Web.Helpers;
using SecSisBoletas.Areas.Empresas.Controllers;
using System.Net;
using System.Collections.Specialized;

namespace SecSisBoletas.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private SecModel db = new SecModel();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // No cuenta los errores de inicio de sesión para el bloqueo de la cuenta
            // Para permitir que los errores de contraseña desencadenen el bloqueo de la cuenta, cambie a shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);

            var user = UserManager.FindByName(model.UserName);

            if (user != null && !UserManager.IsEmailConfirmed(user.Id))
            {
                result = SignInStatus.Failure;
            }

            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Intento de inicio de sesión no válido.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Requerir que el usuario haya iniciado sesión con nombre de usuario y contraseña o inicio de sesión externo
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // El código siguiente protege de los ataques por fuerza bruta a los códigos de dos factores. 
            // Si un usuario introduce códigos incorrectos durante un intervalo especificado de tiempo, la cuenta del usuario 
            // se bloqueará durante un período de tiempo especificado. 
            // Puede configurar el bloqueo de la cuenta en IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Código no válido.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            ViewBag.IdActividad = new SelectList(db.Actividad.OrderBy(x => x.Nombre), "IdActividad", "Nombre");
            ViewBag.IdProvincia = new SelectList(db.Provincia, "IdProvincia", "Nombre",5);
            var localidades = db.Localidad.OrderBy(x => x.Nombre);
            foreach (var loc in localidades)
            {
                loc.NombreCodPostal = loc.Nombre + " (" + loc.CodPostal + ")";
            }
            ViewBag.IdLocalidad = new SelectList(localidades, "IdLocalidad", "NombreCodPostal");
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                CUIT cuit = new CUIT(model.Cuit);
                if (cuit.EsValido)
                {
                    var empresaAux = db.Empresa.Where(x => x.Cuit == model.Cuit).FirstOrDefault();
                    if(empresaAux == null)
                    {
                        var user = new ApplicationUser { UserName = model.Cuit, Email = model.Email };
                        var result = await UserManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            Empresa empresa = new Empresa();
                            empresa.RazonSocial = model.RazonSocial;
                            empresa.NombreFantasia = model.NombreFantasia;
                            empresa.Cuit = model.Cuit;
                            empresa.IdActividad = model.IdActividad;
                            empresa.IdLocalidad = model.IdLocalidad;
                            empresa.Calle = model.Calle;
                            empresa.Altura = model.Altura;
                            empresa.TelefonoFijo = model.TelefonoFijo;
                            empresa.TelefonoCelular = model.TelefonoCelular;
                            empresa.Email = model.Email;
                            empresa.FechaAltaEmpresa = DateTime.Today;
                            db.Empresa.Add(empresa);
                            db.SaveChanges();

                            var currentUser = UserManager.FindByName(user.UserName);

                            var roleresult = UserManager.AddToRole(currentUser.Id, "Empresa");

                            await UserManager.AddClaimAsync(user.Id, new Claim("IdEmpresa", (empresa.IdEmpresa).ToString()));

                            //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                            // Para obtener más información sobre cómo habilitar la confirmación de cuentas y el restablecimiento de contraseña, visite https://go.microsoft.com/fwlink/?LinkID=320771
                            // Enviar correo electrónico con este vínculo
                            string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                            //await UserManager.SendEmailAsync(user.Id, "Confirmar cuenta", "Para confirmar la cuenta, haga clic <a href=\"" + callbackUrl + "\">aquí</a>");

                            var emailBody = "Hola " + empresa.RazonSocial + "!<br /> Para confirmar la cuenta, haga clic <a href=\"" + callbackUrl + "\">aquí</a>";
                            try
                            {
                                //Configuring webMail class to send emails  
                                //gmail smtp server  
                                WebMail.SmtpServer = "mail.xindicoweb.com.ar";
                                //gmail port to send emails  
                                WebMail.SmtpPort = 587;
                                WebMail.SmtpUseDefaultCredentials = true;
                                //sending emails with secure protocol  
                                //WebMail.EnableSsl = true;
                                //EmailId used to send emails from application  
                                WebMail.UserName = "no-reply@xindicoweb.com.ar";//"no-responder@xindicoweb.com.ar";
                                WebMail.Password = "B0L3ZC1ho9vMepG";

                                //Sender email address.  
                                WebMail.From = "no-reply@xindicoweb.com.ar";//"no-responder@xindicoweb.com.ar";

                                //Send email  
                                WebMail.Send(to: user.Email, subject: "Confirmar Cuenta", body: emailBody, isBodyHtml: true);
                            }
                            catch (Exception e)
                            {
                                var AuthenticationManagerAux = HttpContext.GetOwinContext().Authentication;
                                AuthenticationManagerAux.SignOut();

                                db.Empresa.Remove(empresa);
                                db.SaveChanges();
                                UserManager.Delete(user);
                            }

                            var AuthenticationManager = HttpContext.GetOwinContext().Authentication;
                            AuthenticationManager.SignOut();

                            return RedirectToAction("RegisterSuccess");
                        }
                        AddErrors(result);
                    }
                    else
                    {
                        ModelState.AddModelError("Cuit", "Ya Existe una empresa registrada con ese Cuit");
                    }
                    
                }
                else
                {
                    ModelState.AddModelError("Cuit", "Cuit ingresado no es valido");
                }
            }

            // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
            ViewBag.IdActividad = new SelectList(db.Actividad.OrderBy(x => x.Nombre), "IdActividad", "Nombre", model.IdActividad);
            var localidad = db.Localidad.Find(model.IdLocalidad);
            ViewBag.IdProvincia = new SelectList(db.Provincia, "IdProvincia", "Nombre", localidad.IdProvincia);
            var localidades = db.Localidad.OrderBy(x => x.Nombre);
            foreach (var loc in localidades)
            {
                loc.Nombre = loc.Nombre + " (" + loc.CodPostal + ")";
            }
            ViewBag.IdLocalidad = new SelectList(localidades, "IdLocalidad", "Nombre", model.IdLocalidad);

            return View(model);
        }

        public ActionResult CrearUsuario()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearUsuario(string Usuario, string Email, string Rol, string Password)
        {
            if (string.IsNullOrEmpty(Usuario))
            {
                ModelState.AddModelError("User", "El campo Usuario no puede estar vacio");
                return View();
            }

            if (string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("User", "El campo Usuario no puede estar vacio");
                return View();
            }

            var user = new ApplicationUser { UserName = Usuario, Email = Email };
            var result = UserManager.Create(user, Password);
            if (result.Succeeded)
            {
                var result1 = UserManager.AddToRole(user.Id, Rol);
                return RedirectToAction("UsuarioCreado");
            }

            return View();
        }

        public ActionResult UsuarioCreado()
        {
            return View();
        }

        // GET: /Account/RegisterSuccess
        [AllowAnonymous]
        public ActionResult RegisterSuccess()
        {
            return View();
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindByEmail(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // No revelar que el usuario no existe o que no está confirmado
                    return View("ForgotPasswordConfirmation");
                }

                // Para obtener más información sobre cómo habilitar la confirmación de cuentas y el restablecimiento de contraseña, visite https://go.microsoft.com/fwlink/?LinkID=320771
                // Enviar correo electrónico con este vínculo
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                var emailBody = "Hola!<br /> Para restablecer la contraseña, haga clic <a href=\"" + callbackUrl + "\">aquí</a>";

                //Configuring webMail class to send emails  
                //gmail smtp server  
                WebMail.SmtpServer = "mail.xindicoweb.com.ar";
                //gmail port to send emails  
                WebMail.SmtpPort = 587;
                WebMail.SmtpUseDefaultCredentials = true;
                //sending emails with secure protocol  
                //WebMail.EnableSsl = true;
                //EmailId used to send emails from application  
                WebMail.UserName = "no-reply@xindicoweb.com.ar";//"no-responder@xindicoweb.com.ar";
                WebMail.Password = "B0L3ZC1ho9vMepG";

                //Sender email address.  
                WebMail.From = "no-reply@xindicoweb.com.ar";//"no-responder@xindicoweb.com.ar";

                //Send email  
                WebMail.Send(to: user.Email, subject: "Restablecer contraseña", body: emailBody, isBodyHtml: true);
            
                // await UserManager.SendEmailAsync(user.Id, "Restablecer contraseña", "Para restablecer la contraseña, haga clic <a href=\"" + callbackUrl + "\">aquí</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string userId, string code)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var user = context.Users.Find(userId);
            if(user == null)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }
            ViewBag.Email = user.Email;
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // No revelar que el usuario no existe
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Solicitar redireccionamiento al proveedor de inicio de sesión externo
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generar el token y enviarlo
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Si el usuario ya tiene un inicio de sesión, iniciar sesión del usuario con este proveedor de inicio de sesión externo
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // Si el usuario no tiene ninguna cuenta, solicitar que cree una
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Obtener datos del usuario del proveedor de inicio de sesión externo
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Asistentes
        // Se usa para la protección XSRF al agregar inicios de sesión externos
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}