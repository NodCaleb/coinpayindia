#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;

#endregion

namespace CryptoMarket.Source.Managers{
    /// <summary>
    ///     Voucher Manager
    /// </summary>
    public class VouchersManager{
        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="coinId"></param>
        /// <param name="amount"></param>
        /// <param name="expiryDays"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        public static async Task<Vouchers> Create(string userId, string coinId, double amount, int expiryDays, string note){
            using (var context = new ApplicationDbContext()){
                var code = $"IVUGEO-{Guid.NewGuid().ToString().Substring(19)}";

                var newVoucher = new Vouchers{
                    VoucherCode = code,
                    CreatorUserId = userId,
                    Amount = amount,
                    CoinId = coinId,
                    DateCreated = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddDays(expiryDays),
                    Note = note,
                    Redeemed = false
                };

                context.Vouchers.Add(newVoucher);

                await context.SaveChangesAsync();

                return newVoucher;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Vouchers>> GetAll(){
            using (var context = new ApplicationDbContext()){
                return await context.Vouchers.ToListAsync();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="code"></param>
        /// <param name="redeemerUserId"></param>
        /// <returns></returns>
        public static async Task<bool> Redeem(string code, string redeemerUserId){
            using (var context = new ApplicationDbContext()){
                if (!await context.Vouchers.AnyAsync(voucher => voucher.VoucherCode == code))
                    return false;

                var voucherInfo = await context.Vouchers.FirstAsync(voucher => voucher.VoucherCode == code);

                if (voucherInfo.ExpiryDate < DateTime.UtcNow || voucherInfo.Redeemed)
                    return false;

                voucherInfo.RedeemerUserId = redeemerUserId;
                voucherInfo.Redeemed = true;
                voucherInfo.RedeemDate = DateTime.UtcNow;    

                await new BalancesManager(context).DepositAsync(redeemerUserId, voucherInfo.CoinId, voucherInfo.Amount); 

                await context.SaveChangesAsync();

                return true;
            }
        }
    }
}