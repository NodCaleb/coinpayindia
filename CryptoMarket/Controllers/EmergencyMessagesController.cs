using System;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using CryptoMarket.Source;

namespace CryptoMarket.Controllers
{
    [Authorize, Filters.AdminFilter, RequireHttps]
    public class EmergencyMessagesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: EmergencyMessages
        public async Task<ActionResult> Index()
        {
            return View(await db.EmergencyMessages.ToListAsync());
        }

        // GET: EmergencyMessages/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmergencyMessages emergencyMessages = await db.EmergencyMessages.FindAsync(id);
            if (emergencyMessages == null)
            {
                return HttpNotFound();
            }
            return View(emergencyMessages);
        }

        // GET: EmergencyMessages/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EmergencyMessages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,DateCreated,Text,Active")] EmergencyMessages emergencyMessages)
        {
            if (ModelState.IsValid)
            {
                emergencyMessages.Id = Guid.NewGuid();
                emergencyMessages.DateCreated = DateTime.UtcNow;
                db.EmergencyMessages.Add(emergencyMessages);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(emergencyMessages);
        }

        // GET: EmergencyMessages/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmergencyMessages emergencyMessages = await db.EmergencyMessages.FindAsync(id);
            if (emergencyMessages == null)
            {
                return HttpNotFound();
            }
            return View(emergencyMessages);
        }

        // POST: EmergencyMessages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,DateCreated,Text,Active")] EmergencyMessages emergencyMessages)
        {
            if (ModelState.IsValid)
            {
                db.Entry(emergencyMessages).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(emergencyMessages);
        }

        // GET: EmergencyMessages/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmergencyMessages emergencyMessages = await db.EmergencyMessages.FindAsync(id);
            if (emergencyMessages == null)
            {
                return HttpNotFound();
            }
            return View(emergencyMessages);
        }

        // POST: EmergencyMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            EmergencyMessages emergencyMessages = await db.EmergencyMessages.FindAsync(id);
            db.EmergencyMessages.Remove(emergencyMessages);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
