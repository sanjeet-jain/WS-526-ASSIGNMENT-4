﻿using System.Linq;
using System.Threading.Tasks;
using ImageSharingWithCloudStorage.DAL;
using ImageSharingWithCloudStorage.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ImageSharingWithCloudStorage.Controllers;

public class BaseController : Controller
{
    protected ApplicationDbContext db;

    protected UserManager<ApplicationUser> userManager;

    protected BaseController(UserManager<ApplicationUser> userManager,
        ApplicationDbContext db)
    {
        this.db = db;
        this.userManager = userManager;
    }


    protected void CheckAda()
    {
        ViewBag.isADA = GetADAFlag();
    }

    protected bool GetADAFlag()
    {
        var cookie = Request.Cookies["ADA"];
        return cookie != null && "true".Equals(cookie);
    }

    protected async Task<ApplicationUser> GetLoggedInUser()
    {
        //return Request.Cookies["Username"];
        var user = HttpContext.User;
        if (user == null || user.Identity == null || user.Identity.Name == null) return null;
        return await userManager.FindByNameAsync(user.Identity.Name);
    }

    protected ActionResult ForceLogin()
    {
        return RedirectToAction("Login", "Account");
    }

    protected ActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    protected IQueryable<Image> ApprovedImages(IQueryable<Image> images)
    {
        return images.Where(im => im.Approved);
    }

    protected IQueryable<Image> ApprovedImages()
    {
        return ApprovedImages(db.Images);
    }

    protected IQueryable<ApplicationUser> ActiveUsers()
    {
        return userManager.Users.Where(u => u.Active);
    }
}