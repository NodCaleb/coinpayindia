#region

using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using CryptoMarket.Source;
using CryptoMarket.Source.Managers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

#endregion

namespace CryptoMarket.Controllers{
    /// <summary>
    /// </summary>
    [Authorize, Filters.AdminFilter, RequireHttps]
    public class UsersAdminController : Controller{
        /// <summary>
        /// </summary>
        private ApplicationUserManager UserManager{
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }

        /// <summary>
        /// </summary>
        private ApplicationRoleManager RoleManager{
            get { return HttpContext.GetOwinContext().Get<ApplicationRoleManager>(); }
        }


        /// <summary>
        ///     GET: /UsersAdmin/
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index(){
            return View(await UserManager.Users.ToListAsync());
        }

        /// <summary>
        ///     GET: /UsersAdmin/Details/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Details(string id){
            if (id == null){
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);

            ViewBag.RoleNames = await UserManager.GetRolesAsync(user.Id);

            ViewBag.Orders = await ApplicationUserManager.GetUserOrders(user.Id);

            return View(user);
        }


        // GET: /Users/Edit/1
        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Edit(string id){
            if (id == null){
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null){
                return HttpNotFound();
            }

            var userRoles = await UserManager.GetRolesAsync(user.Id);

            return View(new EditUserViewModel{
                Id = user.Id,
                Email = user.Email,
                RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem{
                    Selected = userRoles.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                }),
                Banned = user.LockoutEnabled
            });
        }

        //
        // POST: /Users/Edit/5
        /// <summary>
        /// </summary>
        /// <param name="editUser"></param>
        /// <param name="selectedRole"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Email,Id")] EditUserViewModel editUser, params string[] selectedRole){
            if (ModelState.IsValid){
                var user = await UserManager.FindByIdAsync(editUser.Id);
                if (user == null){
                    return HttpNotFound();
                }

                user.UserName = editUser.Email;
                user.Email = editUser.Email;

                var userRoles = await UserManager.GetRolesAsync(user.Id);

                selectedRole = selectedRole ?? new string[]{};

                var result = await UserManager.AddToRolesAsync(user.Id, selectedRole.Except(userRoles).ToArray());

                if (!result.Succeeded){
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                result = await UserManager.RemoveFromRolesAsync(user.Id, userRoles.Except(selectedRole).ToArray());

                if (!result.Succeeded){
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }

                await LogManager.WriteAsync(Logs.LogType.AdminTrackAction, string.Format("User {0} Edited", user.Email), User.Identity.GetUserId(), Request.UserHostAddress);

                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Something failed.");
            return View();
        }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Ban(string userId){
            await UserManager.SetLockoutEndDateAsync(userId, new DateTimeOffset(new DateTime(2040, 12, 1)));
            await UserManager.SetLockoutEnabledAsync(userId, true);
            return RedirectToAction("Edit", new{id = userId});
        }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> UnBan(string userId){
            await UserManager.SetLockoutEnabledAsync(userId, false);
            return RedirectToAction("Edit", new{id = userId});
        }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> ResetPasword(string userId){
            string message = null;
            //the token is valid for one day
            var until = DateTime.Now.AddHours(12);
            var userInfo = await UserManager.FindByIdAsync(userId);


            var tokenid = await UserManager.GeneratePasswordResetTokenAsync(userId);

            var callbackUrl = Url.Action("ResetPassword", "Account", new{userId, tokenid}, Request.Url.Scheme);

            await UserManager.SendEmailAsync(userId, "Reset Password", "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");

            return RedirectToAction("Edit", new{id = userId});
        }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> ResetPin(string userId){
            var userInfo = await UserManager.FindByIdAsync(userId);
            var newPin = await UsersManager.ResetPin(userId);

            await UserManager.SendEmailAsync(userId, string.Format("Hi, {0}, your new Pin Code", userInfo.UserName), "Administrator resetted your Pin Code. Please, remember your new Pin Code: " + newPin);
            return RedirectToAction("Edit", new{id = userId});
        }

        /// <summary>
        /// </summary>
        public class UsersListExport{
            public string Id { get; set; }
            public string Email { get; set; }
            public string Username { get; set; }
        }
    }
}