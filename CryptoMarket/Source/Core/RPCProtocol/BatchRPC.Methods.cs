#region

using System;
using System.Collections.Generic;

#endregion

namespace CryptoMarket.Source.Core.RPCProtocol{
    public partial class BatchRpc{
        public new uint AddMultiSigAddress(int nRequired, IEnumerable<string> keys, string account = ""){
            var id = NewID;
            _requests.Add
                (new RPCRequest("addmultisigaddress", new Object[]{nRequired, keys, account}, id));
            return id;
        }

        public new uint BackupWallet(string destination){
            var id = NewID;
            _requests.Add
                (new RPCRequest("backupwallet", new Object[]{destination}, id));
            return id;
        }

        public new uint DumpPrivKey(string bitcoinAddress){
            var id = NewID;
            _requests.Add
                (new RPCRequest("dumpprivkey", new Object[]{bitcoinAddress}, id));
            return id;
        }

        public new uint EncryptWallet(string passphrase){
            var id = NewID;
            _requests.Add
                (new RPCRequest("encryptwallet", new Object[]{passphrase}, id));
            return id;
        }

        public new uint GetAccount(string bitcoinAddress){
            var id = NewID;
            _requests.Add
                (new RPCRequest("getaccount", new Object[]{bitcoinAddress}, id));
            return id;
        }

        public new uint GetAccountAddress(string account){
            var id = NewID;
            _requests.Add
                (new RPCRequest("getaccountaddress", new Object[]{account}, id));
            return id;
        }

        public new uint GetAddressesByAccount(string account){
            var id = NewID;
            _requests.Add
                (new RPCRequest("getaddressesbyaccount", new Object[]{account}, id));
            return id;
        }

        public new uint GetBalance(string account = null, int minConf = 1){
            var id = NewID;
            if (account == null){
                _requests.Add
                    (new RPCRequest("getbalance", null, id));
                return id;
            }
            _requests.Add
                (new RPCRequest("getbalance", new Object[]{account, minConf}, id));
            return id;
        }

        public new uint GetBlock(string hash){
            var id = NewID;
            _requests.Add
                (new RPCRequest("getblock", new Object[]{hash}, id));
            return id;
        }

        public new uint GetBlockCount(){
            var id = NewID;
            _requests.Add
                (new RPCRequest("getblockcount", null, id));
            return id;
        }

        public new uint GetBlockHash(long index){
            var id = NewID;
            _requests.Add
                (new RPCRequest("getblockhash", new Object[]{index}, id));
            return id;
        }

        public new uint GetConnectionCount(){
            var id = NewID;
            _requests.Add
                (new RPCRequest("getconnectioncount", null, id));
            return id;
        }

        public new uint GetDifficulty(){
            var id = NewID;
            _requests.Add
                (new RPCRequest("getdifficulty", null, id));
            return id;
        }

        public new uint GetGenerate(){
            var id = NewID;
            _requests.Add
                (new RPCRequest("getgenerate", null, id));
            return id;
        }

        public new uint GetHashesPerSec(){
            var id = NewID;
            _requests.Add
                (new RPCRequest("gethashespersec", null, id));
            return id;
        }

        public new uint GetInfo(){
            var id = NewID;
            _requests.Add
                (new RPCRequest("getinfo", null, id));
            return id;
        }

        public new uint GetMiningInfo(){
            var id = NewID;
            _requests.Add
                (new RPCRequest("getinfo", null, id));
            return id;
        }

        public new uint GetNewAddress(string account = ""){
            var id = NewID;
            _requests.Add
                (new RPCRequest("getnewaddress", new Object[]{account}, id));
            return id;
        }

        public new uint GetReceivedByAccount(string account, int minConf = 1){
            var id = NewID;
            _requests.Add
                (new RPCRequest("getreceivedbyaccount", new Object[]{account, minConf}, id));
            return id;
        }

        public new uint GetReceivedByAddress(string bitcoinAddress, int minConf = 1){
            var id = NewID;
            _requests.Add
                (new RPCRequest("getreceivedbyaddress", new Object[]{bitcoinAddress, minConf}, id));
            return id;
        }

        public new uint GetTransaction(string txID){
            var id = NewID;
            _requests.Add
                (new RPCRequest("gettransaction", new Object[]{txID}, id));
            return id;
        }

        public new uint GetWork(){
            var id = NewID;
            _requests.Add
                (new RPCRequest("getwork", null, id));
            return id;
        }

        public new uint GetWork(string data){
            var id = NewID;
            _requests.Add
                (new RPCRequest("getwork", new Object[]{data}, id));
            return id;
        }

        public new uint Help(string command = ""){
            var id = NewID;
            _requests.Add
                (new RPCRequest("help", new Object[]{command}, id));
            return id;
        }

        public new uint ImportPrivKey(string bitcoinPrivKey, string Label = "", bool Rescan = true){
            var id = NewID;
            _requests.Add
                (new RPCRequest("importprivkey", new Object[]{bitcoinPrivKey, Label, Rescan}, id));
            return id;
        }

        public new uint KeyPoolRefill(){
            var id = NewID;
            _requests.Add
                (new RPCRequest("keypoolrefill", null, id));
            return id;
        }

        public new uint ListAccounts(int MinConf = 1){
            var id = NewID;
            _requests.Add
                (new RPCRequest("listaccounts", new Object[]{MinConf}, id));
            return id;
        }

        public new uint ListAddressGroupings(bool showEmptyGroups = true, bool showEmptyAddresses = true){
            var id = NewID;
            _requests.Add
                (new RPCRequest("listaddressgroupings", new Object[]{showEmptyGroups, showEmptyAddresses}, id));
            return id;
        }

        public new uint ListReceivedByAccount(int MinConf = 1, bool IncludeEmpty = false){
            var id = NewID;
            _requests.Add
                (new RPCRequest("listreceivedbyaccount", new Object[]{MinConf, IncludeEmpty}, id));
            return id;
        }

        public new uint ListReceivedByAddress(int MinConf = 1, bool IncludeEmpty = false){
            var id = NewID;
            _requests.Add
                (new RPCRequest("listreceivedbyaddress", new Object[]{MinConf, IncludeEmpty}, id));
            return id;
        }

        public new uint ListSinceBlock(string BlockHash = null, int TargetConfirmations = 1){
            var id = NewID;
            if (BlockHash == null){
                _requests.Add
                    (new RPCRequest("listsinceblock", null, id));
                return id;
            }
            _requests.Add
                (new RPCRequest("listsinceblock", new Object[]{BlockHash, TargetConfirmations}, id));
            return id;
        }

        public new uint ListTransactions(string Account = "*", int Count = 10, int From = 0){
            var id = NewID;
            _requests.Add
                (new RPCRequest("listtransactions", new Object[]{Account, Count, From}, id));
            return id;
        }

        public new uint Move(string FromAccount, string ToAccount, decimal Amount, int MinConf = 1, string Comment = ""){
            var id = NewID;
            _requests.Add
                (new RPCRequest("move", new Object[]{FromAccount, ToAccount, Amount, MinConf, Comment}, id));
            return id;
        }

        public new uint SendFrom(string FromAccount, string ToBitcoinAddress, decimal Amount, int MinConf = 1, string Comment = "", string CommentTo = ""){
            var id = NewID;
            _requests.Add
                (new RPCRequest("sendfrom", new Object[]{FromAccount, ToBitcoinAddress, Amount, MinConf, Comment, CommentTo}, id));
            return id;
        }

        public new uint SendMany(string FromAccount, IDictionary<string, decimal> ToBitcoinAddresses, int MinConf = 1, string Comment = ""){
            var id = NewID;
            _requests.Add
                (new RPCRequest("sendmany", new Object[]{FromAccount, ToBitcoinAddresses, MinConf, Comment}, id));
            return id;
        }

        public new uint SendToAddress(string BitcoinAddress, decimal Amount, string Comment = "", string CommentTo = ""){
            var id = NewID;
            _requests.Add
                (new RPCRequest("sendtoaddress", new Object[]{BitcoinAddress, Amount, Comment, CommentTo}, id));
            return id;
        }

        public new uint SetAccount(string BitcoinAddress, string Account){
            var id = NewID;
            _requests.Add
                (new RPCRequest("setaccount", new Object[]{BitcoinAddress, Account}, id));
            return id;
        }

        public new uint SetGenerate(bool Generate, int GenProcLimit = 1){
            var id = NewID;
            _requests.Add
                (new RPCRequest("setgenerate", new Object[]{Generate, GenProcLimit}, id));
            return id;
        }

        public new uint SetTxFee(decimal Amount){
            var id = NewID;
            _requests.Add
                (new RPCRequest("settxfee", new Object[]{Amount}, id));
            return id;
        }

        public new uint SignMessage(string BitcoinAddress, string Message){
            var id = NewID;
            _requests.Add
                (new RPCRequest("signmessage", new Object[]{BitcoinAddress, Message}, id));
            return id;
        }

        public new uint Stop(){
            var id = NewID;
            _requests.Add
                (new RPCRequest("stop", null, id));
            return id;
        }

        public new uint ValidateAddress(string Address){
            var id = NewID;
            _requests.Add
                (new RPCRequest("validateaddress", new Object[]{Address}, id));
            return id;
        }

        public new uint WalletLock(){
            var id = NewID;
            _requests.Add
                (new RPCRequest("walletlock", null, id));
            return id;
        }

        public new uint WalletPassphrase(string Passphrase, int Timeout){
            var id = NewID;
            _requests.Add
                (new RPCRequest("walletpassphrase", new Object[]{Passphrase, Timeout}, id));
            return id;
        }

        public new uint WalletPassphraseChange(string OldPassphrase, string NewPassphrase){
            var id = NewID;
            _requests.Add
                (new RPCRequest("walletpassphrasechange", new Object[]{OldPassphrase, NewPassphrase}, id));
            return id;
        }

        public new uint VerifyMessage(string bitcoinAddress, string signature, string message){
            var id = NewID;
            _requests.Add
                (new RPCRequest("verifymessage", new Object[]{bitcoinAddress, signature, message}, id));
            return id;
        }
    }
}