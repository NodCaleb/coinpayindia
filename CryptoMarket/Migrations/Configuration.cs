#region

using System.Data.Entity.Migrations;
using CryptoMarket.Models;

#endregion

namespace CryptoMarket.Migrations{
    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>{
        public Configuration(){
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }
    }
}