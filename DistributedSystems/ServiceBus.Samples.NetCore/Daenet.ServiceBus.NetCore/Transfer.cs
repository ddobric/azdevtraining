using System;
using System.Collections.Generic;
using System.Text;

namespace Daenet.ServiceBus.NetCore
{
    public class Transfer
    {
        public string fromAccount { get; set; }

        public string toAccount { get; set; }

        public decimal amount { get; set; }
    }
}
