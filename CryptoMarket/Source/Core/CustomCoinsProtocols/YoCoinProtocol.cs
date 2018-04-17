using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using CryptoMarket.Source.Managers;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Quartz;

namespace CryptoMarket.Source.Core.CustomCoinsProtocols {
    /// <summary>
    /// 
    /// </summary>
    public class YoCoinProtocol {
        private static readonly string YoCoinContractAddress = "0x37a9679c41e99db270bda88de8ff50c0cd23f326";

        private static readonly string YoCoinABI = "[{\"constant\":true,\"inputs\":[],\"name\":\"name\",\"outputs\":[{\"name\":\"\",\"type\":\"string\"}],\"payable\":false,\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"totalSupply\",\"outputs\":[{\"name\":\"\",\"type\":\"uint256\"}],\"payable\":false,\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"decimals\",\"outputs\":[{\"name\":\"\",\"type\":\"uint8\"}],\"payable\":false,\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_value\",\"type\":\"uint256\"},{\"name\":\"_to\",\"type\":\"address\"}],\"name\":\"chargebackCoins\",\"outputs\":[],\"payable\":false,\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"enabledBlackList\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_contractPays\",\"type\":\"bool\"}],\"name\":\"switchFeePolicy\",\"outputs\":[],\"payable\":false,\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"standard\",\"outputs\":[{\"name\":\"\",\"type\":\"string\"}],\"payable\":false,\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"\",\"type\":\"address\"}],\"name\":\"balanceOf\",\"outputs\":[{\"name\":\"\",\"type\":\"uint256\"}],\"payable\":false,\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"owner\",\"outputs\":[{\"name\":\"\",\"type\":\"address\"}],\"payable\":false,\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"symbol\",\"outputs\":[{\"name\":\"\",\"type\":\"string\"}],\"payable\":false,\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"\",\"type\":\"address\"}],\"name\":\"statusOf\",\"outputs\":[{\"name\":\"\",\"type\":\"uint256\"}],\"payable\":false,\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_value\",\"type\":\"uint256\"},{\"name\":\"_to\",\"type\":\"address\"}],\"name\":\"issueCoins\",\"outputs\":[],\"payable\":false,\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_to\",\"type\":\"address\"},{\"name\":\"_value\",\"type\":\"uint256\"}],\"name\":\"transfer\",\"outputs\":[],\"payable\":false,\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"contractPays\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"newOwner\",\"type\":\"address\"}],\"name\":\"transferOwnership\",\"outputs\":[],\"payable\":false,\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"enabledWhiteList\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_enabledWhiteList\",\"type\":\"bool\"}],\"name\":\"switchWhiteList\",\"outputs\":[],\"payable\":false,\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_enabledBlackList\",\"type\":\"bool\"}],\"name\":\"switchBlackList\",\"outputs\":[],\"payable\":false,\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_address\",\"type\":\"address\"},{\"name\":\"_status\",\"type\":\"uint8\"}],\"name\":\"changeAddressStatus\",\"outputs\":[],\"payable\":false,\"type\":\"function\"},{\"inputs\":[{\"name\":\"initialSupply\",\"type\":\"uint256\"},{\"name\":\"tokenName\",\"type\":\"string\"},{\"name\":\"decimalUnits\",\"type\":\"uint8\"},{\"name\":\"tokenSymbol\",\"type\":\"string\"},{\"name\":\"_contractPays\",\"type\":\"bool\"},{\"name\":\"_enabledWhiteList\",\"type\":\"bool\"},{\"name\":\"_enabledBlackList\",\"type\":\"bool\"}],\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"name\":\"to\",\"type\":\"address\"},{\"indexed\":false,\"name\":\"value\",\"type\":\"int256\"}],\"name\":\"Transfer\",\"type\":\"event\"}]\n";

        private static HexBigInteger _filterHex;

        private static readonly string CoinbaseAccount = "0x9535b2e7faaba5288511d89341d94a38063a349b";

        private static readonly string CoinbaseAccountPassword = "ieuHjfhNheu78Jmd";

        private readonly Web3 _web3;

        private readonly ApplicationDbContext _dbContext;

        public YoCoinProtocol(ApplicationDbContext context) {
            _dbContext = context;
            _web3 = new Web3("http://93.190.142.88:8545");
        }

        public async Task<string> CreateNewAccount(string userId) {
            var keyString = (DateTime.UtcNow.Millisecond + new Random().Next(int.MinValue, int.MaxValue).ToString()).ToMD5();
            var address = await _web3.Personal.NewAccount.SendRequestAsync(keyString);

            //await InitAccount(address);
            _dbContext.EthCoinPrivateKeys.Add(new EthCoinPrivateKeys {
                AccountId = address,
                PrivateKey = keyString
            });



            await _dbContext.SaveChangesAsync();
            return address;
        }

        public async Task InitAccount(string address) {
            await _web3.Personal.UnlockAccount.SendRequestAsync(CoinbaseAccount, CoinbaseAccountPassword, new HexBigInteger(120));
            await _web3.Eth.Transactions.SendTransaction.SendRequestAsync(new TransactionInput(null, address, CoinbaseAccount, new HexBigInteger(50000), new HexBigInteger(UnitConversion.Convert.ToWei(0.0015))));
            await Transfer(address, 0.01);

        }

        public async Task<string> Transfer(string addressTo, double amount) {
            await _web3.Personal.UnlockAccount.SendRequestAsync(CoinbaseAccount, CoinbaseAccountPassword, new HexBigInteger(60));
            return await _web3.Eth.GetContract(YoCoinABI, YoCoinContractAddress).GetFunction("transfer").SendTransactionAsync(CoinbaseAccount, null, null, addressTo, _web3.Convert.ToWei(amount) / 100);
        }


        public class TransferEvent {
            [Parameter("address", "_from", 1, true)]
            public string AddressFrom { get; set; }

            [Parameter("address", "_to", 2, true)]
            public string AddressTo { get; set; }

            [Parameter("uint", "_value", 3)]
            public BigInteger Value { get; set; }
        }

        public static void RegisterTransferFilter() {
            LogManager.WriteToEventLog($"YoCoin filter creating");
            _filterHex = new Web3("http://93.190.142.88:8545").Eth.GetContract(YoCoinABI, YoCoinContractAddress).GetEvent("Transfer").CreateFilterAsync().Result;
            LogManager.WriteToEventLog($"YoCoin filter created: {_filterHex.Value}");
        }

        public class DepositYoCJob : IJob {
            public void Execute(IJobExecutionContext contextExe) {

                foreach (var transferRes in (GetEvents<TransferEvent>(YoCoinContractAddress, YoCoinABI, "Transfer", _filterHex)).Result.ToArray()) {
                    LogManager.WriteToEventLog($"New YOC Event on:{transferRes.AddressTo}, amount:{transferRes.Value}");

                    using (var context = new ApplicationDbContext()) {
                        if (!context.DepositAddressesAssociations.Any(_ => _.Address == transferRes.AddressTo))
                            continue;

                        var addressInfo = context.DepositAddressesAssociations.First(_ => _.Address == transferRes.AddressTo);

                        var userInfo = context.Users.First(_ => _.Id == addressInfo.UserId);

                        var web3 = new Web3("http://93.190.142.88:8545");

                        var yocoinCoin = context.CoinSystems.First(_ => _.ShortName == "YOC");

                        new BalancesManager(context).Deposit(userInfo.Id, yocoinCoin.Id.ToString(), (double) web3.Convert.FromWei(transferRes.Value, 16));

                        context.SaveChanges();
                    }
                }

            }

        }


        public static async Task<List<T>> GetEvents<T>(string address, string abi, string eventName, HexBigInteger filter) where T : new() {
            var contract = new Web3("http://93.190.142.88:8545").Eth.GetContract(abi, address);
            var ev = contract.GetEvent(eventName);
            var events = await ev.GetFilterChanges<T>(filter);
            return events?.GroupBy(o => new {
                o.Log.Address,
                o.Log.Data
            }).Select(o => o.First()).Select(o => o.Event).ToList() ?? new List<T>();
        }

    }
}