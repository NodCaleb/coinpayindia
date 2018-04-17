using CryptoMarket.Source.Managers;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace TestEmail
{
    class Program
    {
        static void Main(string[] args)
        {
            var rpcInit = CoinsRpcManager.Init("3d7c58fc-5205-e411-80b9-0cc47a02cce9");

            var bal = rpcInit.GetBalance();
        }


    }
}