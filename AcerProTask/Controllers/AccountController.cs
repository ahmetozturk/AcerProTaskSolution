using AcerProTask.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcerProTask.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _passwordHasher = passwordHasher;

        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {

            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme); // Identity.Application
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme); // Identity.External

            return RedirectToAction("RegisterAndLogin", "Account");
        }

        [HttpGet]
        public IActionResult RegisterAndLogin()
        {
            var model = new RegisterAndLoginViewModel
            {
                RegisterModel = new RegisterViewModel(),
                LoginModel = new LoginViewModel()
            };
            return View(model);
        }
        static public bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ModelStateRemoveLogin()
        {
            ModelState.Remove("LoginModel.EmailorUserName");
            ModelState.Remove("LoginModel.Password");
        }

        public void ModelStateRemoveRegister()
        {
            ModelState.Remove("RegisterModel.Email");
            ModelState.Remove("RegisterModel.Password");
            ModelState.Remove("RegisterModel.UserName");
            ModelState.Remove("RegisterModel.PhoneNumber");
            ModelState.Remove("RegisterModel.Password");
            ModelState.Remove("RegisterModel.ConfirmPassword");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAndLogin(RegisterAndLoginViewModel model, string actionType)
        {
            TempData["ActionType"] = actionType;

            if (actionType == "Register")
            {
                ModelStateRemoveLogin();
                if (!TryValidateModel(model.RegisterModel, nameof(model.RegisterModel)))
                {
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        ModelState.AddModelError("", error.ErrorMessage);
                    }
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.RegisterModel.UserName,
                    EmailConfirmed = true,
                    Email = model.RegisterModel.Email,
                    NormalizedEmail = model.RegisterModel.Email.ToUpper(),
                    NormalizedUserName = model.RegisterModel.UserName.ToUpper(),
                    PhoneNumber = model.RegisterModel.PhoneNumber,
                };

                var passwordHash = _passwordHasher.HashPassword(user, model.RegisterModel.Password);
                user.PasswordHash = passwordHash;

                var result = await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            else if (actionType == "Login")
            {
                ModelStateRemoveRegister();



                if (!TryValidateModel(model.LoginModel, nameof(model.LoginModel)))
                {
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        ModelState.AddModelError("", error.ErrorMessage);
                    }
                    return View(model);
                }

                bool isMail = IsValidEmail(model.LoginModel.EmailorUserName);
                var user = new ApplicationUser();
                if (isMail)
                {
                    user = await _userManager.FindByEmailAsync(model.LoginModel.EmailorUserName);
                }
                else
                {
                    user = await _userManager.FindByNameAsync(model.LoginModel.EmailorUserName);
                }



                if (user != null)
                {
                    var hasher = new PasswordHasher<ApplicationUser>();
                    var verificationResult = hasher.VerifyHashedPassword(user, user.PasswordHash, model.LoginModel.Password);

                    if (verificationResult == PasswordVerificationResult.Success)
                    {

                        await _signInManager.SignInAsync(user, isPersistent: false);

                    }
                    else
                    {

                        ViewBag.LoginError = "Invalid username or password.";
                        return View(model);
                    }


                }



                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
    }




}
