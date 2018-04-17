using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CryptoMarket.Source.Core{
    public enum OrderState{
        WaitForTrigger,
        Opened,
        Closed,
        PartialClosed,
        Cancelled
    }
}