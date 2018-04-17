using System;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using CryptoMarket.Source;

namespace CryptoMarket.Controllers{
    [Authorize, Filters.AdminFilter]
    public class BlogAdminController : Controller{
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BlogAdmin
        public async Task<ActionResult> Index(){
            return View(await db.BlogPosts.ToListAsync());
        }

        // GET: BlogAdmin/Details/5
        public async Task<ActionResult> Details(Guid? id){
            if (id == null){
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPosts blogPosts = await db.BlogPosts.FindAsync(id);
            if (blogPosts == null){
                return HttpNotFound();
            }
            return View(blogPosts);
        }

        // GET: BlogAdmin/Create
        public ActionResult Create(){
            return View();
        }

        // POST: BlogAdmin/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Create([Bind(Include = "Title,Text")] BlogPosts blogPosts){
            if (ModelState.IsValid){
                blogPosts.Id = Guid.NewGuid();
                blogPosts.DateTime = DateTime.UtcNow;
                db.BlogPosts.Add(blogPosts);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(blogPosts);
        }

        // GET: BlogAdmin/Edit/5
        public async Task<ActionResult> Edit(Guid? id){
            if (id == null){
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPosts blogPosts = await db.BlogPosts.FindAsync(id);
            if (blogPosts == null){
                return HttpNotFound();
            }
            return View(blogPosts);
        }

        // POST: BlogAdmin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,DateTime,Title,Text")] BlogPosts blogPosts){
            if (ModelState.IsValid){
                db.Entry(blogPosts).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(blogPosts);
        }

        // GET: BlogAdmin/Delete/5
        public async Task<ActionResult> Delete(Guid? id){
            if (id == null){
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPosts blogPosts = await db.BlogPosts.FindAsync(id);
            if (blogPosts == null){
                return HttpNotFound();
            }
            return View(blogPosts);
        }

        // POST: BlogAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id){
            BlogPosts blogPosts = await db.BlogPosts.FindAsync(id);
            db.BlogPosts.Remove(blogPosts);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing){
            if (disposing){
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}