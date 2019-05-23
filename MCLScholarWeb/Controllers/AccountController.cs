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
using MCLScholarWeb.Models.Entity;
using MCLScholarWeb.Models.Entity.Account;
using System.Collections.Generic;
using System.Web.Services;
using MCLScholarWeb.Actions;

namespace MCLScholarWeb.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private lipatdbEntities db;
        public AccountController()
        {
            db = new lipatdbEntities();
        }
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> AddAllStudents()
        {
            List<StudentProfile> profile = db.StudentProfiles.ToList();
            int count = 0;

            foreach (var item in profile)
            {
                if (db.AspNetUsers.Where(e => e.Email.Equals(item.Email)).FirstOrDefault() == null)
                {
                    RegisterViewModel viewModel = new RegisterViewModel()
                    {
                        Email = item.Email,
                        ConfirmPassword = "changeme",
                        Password = "changeme"
                    };
                    await RegisterAccountAsync(viewModel);
                    count++;
                }
            }
            TempData["Message"] = string.Format("{0} Accounts Added", count);
            TempData["MessageType"] = "success";

            return RedirectToAction("Accounts", "Admin", new { role = "Student" });

        }


        public ActionResult Details(string id)
        {
            var user = db.AspNetUsers.Where(e => e.Id.Equals(id)).FirstOrDefault();
            if (user == null)
            {
                ViewBag.Message = "Cannot find account";
                ViewBag.MessageType = "error";
                return View();
            }
            return View(user);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteAccount(string id)
        {
           
            //Validate ID
            var user = db.AspNetUsers.Where(e => e.Id.Equals(id)).FirstOrDefault();
            if (user == null)
            {
                TempData["Message"] = "Failed Deleting Account: Cannot find account";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Accounts", "Admin");
            }
            string role = user.AspNetUserRoles.FirstOrDefault().AspNetRole.Name;
            string userRoleID = user.AspNetUserRoles.FirstOrDefault().RoleId;
            int roleCount = db.AspNetUserRoles.Where(e => e.RoleId.Equals(userRoleID)).Count();
            if (roleCount <= 1 && role.Equals("Administrator"))
            {
                TempData["Message"] = "Failed Deleting Account: Cannot delete last account";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Accounts", "Admin", new { role = role });
            }

            db.AspNetUsers.Remove(user);
            db.SaveChanges();
            TempData["Message"] = "Account Successfuly Deleted";
            TempData["MessageType"] = "success";
            return RedirectToAction("Accounts", "Admin", new { role = role });
        }








        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterAsync(EmailModel model)
        {
            string email = model.Email;
            if (db.StudentProfiles.Where(e => e.Email.Equals(email)).FirstOrDefault() == null)
            {
                ViewBag.ErrorMessage = "Not a valid MCL student email";
                return View();
            };
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            string password = "ChangeMe1";
            RegisterViewModel viewModel = new RegisterViewModel() {
                ConfirmPassword = password,
                Email = model.Email,
                Password = password
            };
            await RegisterAccountAsync(viewModel);
            return RedirectToAction("Accounts", "Admin", new { role = "Student" });
        }


        [AllowAnonymous]
        public ActionResult RegisterDetails()
        {
            EmailModel mod = (EmailModel)TempData["emailModel"];
            if (mod == null)
            {
                return RedirectToAction("Register");
            }

            RegisterViewModel viewModel = new RegisterViewModel
            {
                Email = mod.Email
            };
            return View(viewModel);
        }

        public class Year
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult GetYearByID(int value)
        {
            List<Year> ret = new List<Year>();
            Program program = db.Programs.Where(prg => prg.ProgramID.Equals(value)).FirstOrDefault();
            int maxYear = program.MaxYear;
            for (int i = 1; i <= maxYear; i++)
            {
                ret.Add(new Year()
                {
                    Text = i.ToString(),
                    Value = i.ToString()
                });
            }

            return Json(ret, JsonRequestBehavior.AllowGet);
        }


        public async Task RegisterAccountAsync(RegisterViewModel model)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await UserManager.CreateAsync(user, model.Password);

            //create user profile
            AspNetUser netUser = db.AspNetUsers.Where(tuser => tuser.Id.Equals(user.Id)).FirstOrDefault();
            netUser.PeriodID = WebSettings.CurrentPeriod().PeriodID;
            netUser.ValidationPeriod = db.ValidationPeriods.Where(e => e.PeriodID == netUser.PeriodID).FirstOrDefault();
            StudentProfile studentProfile = db.StudentProfiles.Where(e => e.Email.Equals(model.Email)).FirstOrDefault();

            //Assign Profile
            UserProfile userProfile = new UserProfile();
            userProfile.AspNetUser = netUser;
            userProfile.StudentProfile = studentProfile;
            userProfile.ProfileID = studentProfile.ProfileID;
            userProfile.UserID = netUser.Id;
            db.UserProfiles.Add(userProfile);

            // Assign user role   
            AspNetRole role = db.AspNetRoles.Where(trole => trole.Name.Equals("Student")).FirstOrDefault();
            AspNetUserRole userRole = new AspNetUserRole
            {
                AspNetRole = role,
                AspNetUser = netUser,
                RoleId = role.Id,
                UserId = user.Id
            };
            db.AspNetUserRoles.Add(userRole);

            // add user program
            int programID = db.Programs.Where(e => e.ProgramName.Equals(studentProfile.Program)).FirstOrDefault().ProgramID;
            int year = studentProfile.Year;
            Program program = db.Programs.Where(tPrg => tPrg.ProgramID.Equals(programID)).FirstOrDefault();
            UserProgram userProgram = new UserProgram()
            {
                AspNetUser = netUser,
                ProgramID = programID,
                Program = program,
                UserID = user.Id,
                Year = year
            };

            db.UserPrograms.Add(userProgram);

            if (!result.Succeeded)
            {
                AddErrors(result);
            }

            db.SaveChanges();

            // Send Verification link
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
            await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");


        }

        // POST: /Account/Register
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> RegisterDetails(RegisterViewModel model)
        {
            //error handling
            if (!ModelState.IsValid)
            {

                ViewBag.Error = "";
                return View(model);
            }
            await RegisterAccountAsync(model);
            return RedirectToAction("Accounts","Admin",new { role = "Student" });
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> CreateNonStudent(RegisterViewModel model, string role)
        {
            //error handling
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await UserManager.CreateAsync(user, model.Password);

            //create user profile
            AspNetUser netUser = db.AspNetUsers.Where(tuser => tuser.Email.Equals(model.Email)).FirstOrDefault();

            // Assign user role   
            AspNetRole netRole = db.AspNetRoles.Where(trole => trole.Name.Equals(role)).SingleOrDefault();
            AspNetUserRole userRole = new AspNetUserRole();


            userRole.AspNetRole = netRole;
            userRole.AspNetUser = netUser;
            userRole.RoleId = netRole.Id;
            userRole.UserId = netUser.Id;

            db.AspNetUserRoles.Add(userRole);
            db.SaveChanges();

            return RedirectToAction("Accounts", "Admin", new { role = role });
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

            if (User.Identity.IsAuthenticated)
            {
                return Redirect(Url.Content("~/"));
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

            //if (!lipatdb.AspNetUsers.Where(user => user.Email.Equals(model.Email)).FirstOrDefault().EmailConfirmed)
            //{
            //    Session.Abandon();
            //    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            //    ModelState.AddModelError("", "Please verify your email");
            //    return View(model);
            //}

            switch (result)
            {
                case SignInStatus.Success:
                    Session["Period"] = db.ValidationPeriods.OrderByDescending(e => e.AcademicYearStart).FirstOrDefault();
                    return RedirectToAction("Announcement", "WebsiteContent");
                //return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Please Check Your Email or Password");
                    return View(model);
            }
        }





        //-----------------------------------GENERATED CODE-----------------------------------------------

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
            if (User.Identity.IsAuthenticated)
            {
                return Redirect(Url.Content("~/"));
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }



        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
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

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [HttpGet]
        public ActionResult Register()
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
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
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
        public ActionResult ResetPassword(string code)
        {
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
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
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
            // Request a redirect to the external login provider
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

            // Generate the token and send it
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

            // Sign in the user with this external login provider if the user already has a login
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
                    // If the user does not have an account, then prompt the user to create an account
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
                // Get the information about the user from the external login provider
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

        //   POST: /Account/LogOff

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return Redirect(Url.Content("~/"));
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


        #region Helpers
        // Used for XSRF protection when adding external logins
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