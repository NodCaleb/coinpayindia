#region

using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;

#endregion

namespace CryptoMarket.Source.Managers{
    /// <summary>
    /// </summary>
    public class GlobalFreezeManager{
        private static bool? IsGlobalFreezedValue { get; set; }

        /// <summary>
        /// </summary>
        public bool IsGlobalFreezed{
            get{
                if (IsGlobalFreezedValue.HasValue){
                    return IsGlobalFreezedValue.Value;
                }

                using (var context = new ApplicationDbContext()){
                    IsGlobalFreezedValue = context.WebsiteBooleanStates.First(state => state.Name == typeof (GlobalFreezeManager).Name).State;
                    return IsGlobalFreezedValue.Value;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="freeze"></param>
        /// <returns></returns>
        public static async Task ChangeState(bool freeze){
            var stateName = typeof (GlobalFreezeManager).Name;

            IsGlobalFreezedValue = freeze;

            using (var context = new ApplicationDbContext()){
                if (!await context.WebsiteBooleanStates.AnyAsync(state => state.Name == stateName)){
                    context.WebsiteBooleanStates.Add(new WebsiteBooleanStates{
                        Name = stateName,
                        State = freeze
                    });

                    await context.SaveChangesAsync();
                }

                var stateData = await context.WebsiteBooleanStates.FirstAsync(state => state.Name == stateName);

                stateData.State = freeze;

                context.Entry(stateData).State = EntityState.Modified;

                await context.SaveChangesAsync();
            }
        }
    }
}