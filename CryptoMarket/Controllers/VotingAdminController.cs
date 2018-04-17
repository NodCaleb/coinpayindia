#region

using System;
using System.Data.Entity;
using System.Linq;
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
    [Authorize, Filters.AdminFilter, RequireHttps]
    public class VotingAdminController : Controller{
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        
        /// <summary>
        /// GET: VotingAdmin
        /// </summary>
        public async Task<ActionResult> Index(){
            var totalVoteCount = 0.0;
            if (_db.VotingForCoins.Any()){
                totalVoteCount = _db.VotingForCoins.Sum(vote => vote.CurrentVotesNumber);
            }
            ViewBag.TotalVoteCount = totalVoteCount;
            ViewBag.TotalBtc = totalVoteCount * 0.0005;
            return View(await _db.VotingForCoins.ToListAsync());
        }

       
        /// <summary>
        /// GET: VotingAdmin/Details/5
        /// </summary>
        /// <param name="id"></param>
        public async Task<ActionResult> Details(Guid? id){
            if (id == null){
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var votingForCoins = await _db.VotingForCoins.FindAsync(id);
            if (votingForCoins == null){
                return HttpNotFound();
            }
            return View(votingForCoins);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public async Task<ActionResult> ToggleActive(Guid? id){
            var votingForCoins = await _db.VotingForCoins.FindAsync(id);
            if (votingForCoins == null){
                return HttpNotFound();
            }

            votingForCoins.Active = !votingForCoins.Active;

            _db.Entry(votingForCoins).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: VotingAdmin/Create
        /// <summary>
        /// </summary>
        public async Task<ViewResult> Create(){
            ViewBag.CoinSystemsList = await _db.CoinSystems.ToListAsync();
            return View();
        }

        /// <summary>
        ///     POST: VotingAdmin/Create
        /// </summary>
        /// <param name="votingForCoins"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(VotingForCoins votingForCoins){
            if (ModelState.IsValid){
                var account = $"vote-{votingForCoins.CoinName}-{Guid.NewGuid()}";
                
                votingForCoins.VotingAddress = CoinsRpcManager.Init(_db.CoinSystems.First(systems => systems.Id.ToString() == votingForCoins.CoinUsed).Id.ToString()).GetNewAddress(account);
                votingForCoins.CurrentVotesNumber = 0;
                votingForCoins.VotingAccount = account; 
                _db.VotingForCoins.Add(votingForCoins);

                await _db.SaveChangesAsync();

                await LogManager.WriteAsync(Logs.LogType.AdminTrackAction, $"New Voting Created:{votingForCoins.CoinName}", User.Identity.GetUserId(), Request.UserHostAddress);

                return RedirectToAction("Index");
            }

            return View(votingForCoins);
        }


        // GET: VotingAdmin/Delete
        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Delete(Guid? id){
            if (id == null){
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var votingForCoins = await _db.VotingForCoins.FindAsync(id);
            if (votingForCoins == null){
                return HttpNotFound();
            }
            return View(votingForCoins);
        }

        /// <summary>
        /// POST: VotingAdmin/Delete
        /// </summary>
        /// <param name="id"></param>
        [HttpPost, ActionName("Delete"),ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id){
            var votingForCoins = await _db.VotingForCoins.FindAsync(id);
            _db.VotingForCoins.Remove(votingForCoins);
            await _db.SaveChangesAsync();

            await LogManager.WriteAsync(Logs.LogType.AdminTrackAction, string.Format("Voting Deleted:{0}", votingForCoins.CoinName), User.Identity.GetUserId(), Request.UserHostAddress);

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