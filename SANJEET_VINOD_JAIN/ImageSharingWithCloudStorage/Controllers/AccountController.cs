﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using ImageSharingWithCloudStorage.DAL;
using ImageSharingWithCloudStorage.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace ImageSharingWithCloudStorage.Controllers
{
    // TODO-DONE
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class AccountController : BaseController
    {
        protected SignInManager<ApplicationUser> signInManager;

        private readonly ILogger<AccountController> logger;

        protected IImageStorage images;

        // Dependency injection of DB context and user/signin managers
        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 ApplicationDbContext db,
                                 IImageStorage images,
                                 ILogger<AccountController> logger) 
            : base(userManager, db)
        {
            this.signInManager = signInManager;
            this.logger = logger;
            this.images = images;
        }


        // TODO-DONE 
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            CheckAda();
            return View();
        }

        // TODO-DONE 
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            CheckAda();

            if (ModelState.IsValid)
            {
                logger.LogDebug("Registering user: " + model.Email);
                IdentityResult result = null;
                // TODO-DONE register the user from the model, and log them in
                var user = new ApplicationUser(model.Email, model.ADA);
                result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    SaveADACookie(model.ADA);
                    await signInManager.SignInAsync(user, false);
                    logger.LogDebug("Registration Success");
                    return RedirectToAction("Index", "Home", new { model.Email });
                }
                else
                {
                    logger.LogDebug("Registration Failed");
                    ModelState.AddModelError(string.Empty, "Registration failed");
                }

            }

            // If we got this far, something failed, redisplay form
            return View(model);

        }

        // TODO-DONE 
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            CheckAda();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // TODO-DONE 
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl)
        {
            CheckAda();
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // TODO-DONE log in the user from the model (make sure they are still active)
            var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
            if (result.Succeeded)
            {
                var user = await userManager.FindByNameAsync(model.UserName);
                if (user.Active)
                {
                    logger.LogDebug("Login Success for {0}", model.UserName);
                    SaveADACookie(user.ADA);
                    return Redirect(returnUrl ?? "/");
                }
                else
                {
                    logger.LogDebug("Login Fail for {0}", model.UserName);
                    await signInManager.SignOutAsync();
                    ViewBag.Message = "User is INACTIVE!";
                    return View(model);
                }
            }
            logger.LogDebug("Login Fail for {0}", model.UserName);
            ViewBag.Message = "Incorrect Credentials";

            return View(model);
        }

        // TODO-DONE 
        [HttpGet]
        public ActionResult Password(PasswordMessageId? message)
        {
            CheckAda();
            ViewBag.StatusMessage =
                 message == PasswordMessageId.ChangePasswordSuccess ? "Your password has been changed."
                 : message == PasswordMessageId.SetPasswordSuccess ? "Your password has been set."
                 : message == PasswordMessageId.RemoveLoginSuccess ? "The external login was removed."
                 : "";
            ViewBag.ReturnUrl = Url.Action("Password");
            return View();
        }

        // TODO-DONE 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Password(LocalPasswordModel model)
        {
            CheckAda();

            ViewBag.ReturnUrl = Url.Action("Password");
            if (ModelState.IsValid)
            {
                IdentityResult idResult = null; ;

                // TODO-DONE change the password
                var user = await GetLoggedInUser();
                if (user == null)
                {
                    logger.LogDebug("Access Denied Error for {0}", user.UserName);
                    return RedirectToAction("AccessDenied");
                }

                var checkPassword = await userManager.CheckPasswordAsync(user, model.OldPassword);
                if (checkPassword)
                {
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                    idResult = await userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

                    if (idResult.Succeeded)
                    {
                        logger.LogDebug("Password reset success for {0}", user.UserName);
                        return RedirectToAction("Password", new { Message = PasswordMessageId.ChangePasswordSuccess });
                    }
                    ModelState.AddModelError("", "The new password is invalid.");
                }
                else
                {
                    logger.LogDebug("Incorrect Password attempt for {0}", user.UserName);
                    ModelState.AddModelError("OldPassword", "The old password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // TODO-DONE
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Manage()
        {
            CheckAda();

            List<SelectListItem> users = new List<SelectListItem>();
            foreach (var u in db.Users)
            {
                SelectListItem item = new SelectListItem { Text = u.UserName, Value = u.Id, Selected = u.Active };
                users.Add(item);
            }

            ViewBag.message = "";
            ManageModel model = new ManageModel { Users = users };
            return View(model);
        }

        // TODO-DONE
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(ManageModel model)
        {
            CheckAda();

            foreach (var userItem in model.Users)
            {
                ApplicationUser user = await userManager.FindByIdAsync(userItem.Value);

                // Need to reset user name in view model before returning to user, it is not posted back
                userItem.Text = user.UserName;

                if (user.Active && !userItem.Selected)
                {
                    var images = db.Entry(user).Collection(u => u.Images).Query().ToList();
                    foreach (Image image in images)
                    {
                        // TODO Remove the image in blob storage.
                        await this.images.DeleteFileAsync(image.Id);
                        db.Images.Remove(image);
                    }
                    user.Active = false;
                }
                else if (!user.Active && userItem.Selected)
                {
                    /*
                     * Reactivate a user
                     */
                    user.Active = true;
                }
            }
            await db.SaveChangesAsync();

            ViewBag.message = "Users successfully deactivated/reactivated";

            return View(model);
        }

        // TODO-DONE 
        [HttpGet]
        public async Task<IActionResult> Logoff()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> AccessDenied()
        {
            CheckAda();
            return View();
        }

        protected void SaveADACookie(bool value)
        {
            // TODO-DONE save the value in a cookie field key
            var options = new CookieOptions
                { IsEssential = true, Secure = true, SameSite = SameSiteMode.None, Expires = DateTime.Now.AddMonths(3) };
            Response.Cookies.Append("ADA", value.ToString().ToLower(), options);
        }

        public enum PasswordMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

    }
}
