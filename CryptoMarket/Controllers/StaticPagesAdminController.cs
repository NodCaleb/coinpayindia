#region

using System;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using CryptoMarket.Source;
using CryptoMarket.Source.Managers;
using Microsoft.AspNet.Identity;

#endregion

namespace CryptoMarket.Controllers{
    /// <summary>
    /// </summary>
   [Authorize, Filters.AdminFilter]
    public class StaticPagesAdminController : Controller{
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // GET: StaticPagesAdmin
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index(){
            return View(await _db.StaticPages.ToListAsync());
        }


        // GET: StaticPagesAdmin/Create
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public ActionResult Create(){
            return View();
        }

        // POST: StaticPagesAdmin/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// </summary>
        /// <param name="staticPages"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Create([Bind(Include = "Url,Title,Text,LastModified,UserIdLastChanger")] StaticPages staticPages){
            if (ModelState.IsValid){
                staticPages.Id = Guid.NewGuid();
                staticPages.LastModified = DateTime.UtcNow;
                staticPages.UserIdLastChanger = User.Identity.GetUserId();
                _db.StaticPages.Add(staticPages);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            await LogManager.WriteAsync(Logs.LogType.AdminTrackAction, string.Format("Static Page Create, Title:{0}", staticPages.Title), User.Identity.GetUserId(), Request.UserHostAddress);

            return View(staticPages);
        }

        // GET: StaticPagesAdmin/Edit/5
        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Edit(Guid? id){
            if (id == null){
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var staticPages = await _db.StaticPages.FindAsync(id);
            if (staticPages == null){
                return HttpNotFound();
            }
            return View(staticPages);
        }

        // POST: StaticPagesAdmin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// </summary>
        /// <param name="staticPages"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Url,Title,Text,LastModified,UserIdLastChanger")] StaticPages staticPages){
            staticPages.LastModified = DateTime.UtcNow;
            staticPages.UserIdLastChanger = User.Identity.GetUserId();

            if (ModelState.IsValid){
                _db.Entry(staticPages).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            await LogManager.WriteAsync(Logs.LogType.AdminTrackAction, string.Format("Static Page Edit, Title:{0}", staticPages.Title), User.Identity.GetUserId(), Request.UserHostAddress);

            return View(staticPages);
        }

        // GET: StaticPagesAdmin/Delete/5
        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Delete(Guid? id){
            if (id == null){
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var staticPages = await _db.StaticPages.FindAsync(id);
            if (staticPages == null){
                return HttpNotFound();
            }
            return View(staticPages);
        }

        // POST: StaticPagesAdmin/Delete/5
        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id){
            var staticPages = await _db.StaticPages.FindAsync(id);
            _db.StaticPages.Remove(staticPages);
            await _db.SaveChangesAsync();

            await LogManager.WriteAsync(Logs.LogType.AdminTrackAction, string.Format("Static Page Delete, Title:{0}", staticPages.Title), User.Identity.GetUserId(), Request.UserHostAddress);

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing){
            if (disposing){
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}