﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Abc.MvcWebUI.Entity;
using Abc.MvcWebUI.Identity;
using Abc.MvcWebUI.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using PagedList;

namespace Abc.MvcWebUI.Controllers
{
    public class AccountController : Controller
    {
        private DataContext db = new DataContext();

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AccountController()
        {
            var userStore = new UserStore<ApplicationUser>(new IdentityDataContext());
            _userManager = new UserManager<ApplicationUser>(userStore);

            var roleStore = new RoleStore<ApplicationRole>(new IdentityDataContext());
            _roleManager = new RoleManager<ApplicationRole>(roleStore);

        }

        [Authorize]
        public ActionResult Index(int? SayfaNo)
        {
            var username = User.Identity.Name;
            var orders = db.Orders
                .Where(i => i.Username == username)
                .Select(i => new UserOrderModel()
                {
                    Id = i.Id,
                    OrderNumber = i.OrderNumber,
                    OrderDate = i.OrderDate,
                    OrderState = i.OrderState,
                    Total = i.Total
                }).OrderByDescending(i => i.OrderDate).ToList();


            //var comment = db.Commments.AsNoTracking().ToList(); //Veritabanında değişiklik olmadığı için AsNoTracking
            //if (!User.IsInRole("admin"))
            //    comment = comment.Where(i => i.UserName == username).ToList();
            //ViewBag.comment = comment;

            if (Request.IsAjaxRequest())
            {
                int _sayfaNo = SayfaNo ?? 1;
                var comment = db.Commments.AsNoTracking().ToList().Where(i => i.UserName == User.Identity.Name).ToPagedList<Comment>(_sayfaNo, 5);
                if (User.IsInRole("admin"))
                {
                    comment = db.Commments.AsNoTracking().ToList().ToPagedList<Comment>(_sayfaNo, 5);
                }
                return PartialView("~/Views/Comment/_ShowCommentsbyUser.cshtml", comment);
                //RedirectToAction("ShowCommentsbyUser", "Comment", new { username = username, SayfaNo = SayfaNo });
            }
            return View(orders);
        }
        [Authorize]
        public ActionResult Details(int id)
        {
            var entity = db.Orders.Where(i => i.Id == id)
                .Select(i => new OrderDetailsModel()
                {
                    OrderId = i.Id,
                    OrderNumber = i.OrderNumber,
                    Total = i.Total,
                    OrderDate = i.OrderDate,
                    OrderState = i.OrderState,
                    AdresBasligi = i.AdresBasligi,
                    Adres = i.Adres,
                    Sehir = i.Sehir,
                    Semt = i.Semt,
                    Mahalle = i.Mahalle,
                    PostaKodu = i.PostaKodu,
                    Orderlines = i.Orderlines.Select(a => new OrderLineModel()
                    {
                        ProductId = a.ProductId,
                        ProductName = a.Product.Name.Length > 50 ? a.Product.Name.Substring(0, 47) + "..." : a.Product.Name,
                        Image = a.Product.Image,
                        Quantity = a.Quantity,
                        Price = a.Price
                    }).ToList()
                }).FirstOrDefault();

            return View(entity);
        }

        // GET: Account
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Register model)
        {
            if (ModelState.IsValid) //validationlar kontrol ediliyor
            {
                //Kayıt işlemleri

                var user = new ApplicationUser();
                user.Name = model.Name;
                user.Surname = model.SurName;
                user.Email = model.Email;
                user.UserName = model.UserName;

                var result = _userManager.Create(user, model.Password);

                if (result.Succeeded)
                {
                    //kullanıcı oluştu ve kullanıcıyı bir role atayabilirsiniz.
                    if (_roleManager.RoleExists("user"))
                    {
                        _userManager.AddToRole(user.Id, "user");
                    }
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    ModelState.AddModelError("RegisterUserError", "Kullanıcı  oluşturma hatası.");
                }

            }

            return View(model);
        }

        // GET: Account
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Login model, string ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                //Login işlemleri
                var user = _userManager.Find(model.UserName, model.Password);

                if (user != null)
                {
                    // varolan kullanıcıyı sisteme dahil et.
                    // ApplicationCookie oluşturup sisteme bırak.

                    var authManager = HttpContext.GetOwinContext().Authentication;
                    var identityclaims = _userManager.CreateIdentity(user, "ApplicationCookie");
                    var authProperties = new AuthenticationProperties();
                    authProperties.IsPersistent = model.RememberMe; //Login modelindeki rememberMe beni hatırla, cookie nin kalıcı olup olmayacağını belirler
                    authManager.SignIn(authProperties, identityclaims);

                    if (!String.IsNullOrEmpty(ReturnUrl)) //Login olmadan girilemeyen sflardan gelen login ekranında,
                    {                                      //login olduktan sonra returnURL ile login olduktan sonra tekrar gidilmek istenen sf açılır 
                    
                        return Redirect(ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("LoginUserError", "Böyle bir kullanıcı yok.");
                }
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            var authManager = HttpContext.GetOwinContext().Authentication;
            authManager.SignOut();

            return RedirectToAction("Index", "Home");
        }

    }
}