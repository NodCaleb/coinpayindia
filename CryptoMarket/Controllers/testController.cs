using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CryptoMarket.Source.Managers;

namespace CryptoMarket.Controllers
{
    public class testController : Controller
    {
        // GET: test
        public void Index() {
            VerificationManager.SendSMS("+79999822185", "12345");
        }
    }
}