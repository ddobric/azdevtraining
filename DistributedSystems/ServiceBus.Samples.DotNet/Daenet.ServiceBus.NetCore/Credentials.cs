using Microsoft.Azure.Management.ServiceBus.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daenet.ServiceBus.NetCore
{
    public class Credentials
    {
        public static Credentials Current { get { return new Credentials(); } }
        public string ConnStr { get  { return "Endpoint=sb://daenettraining.servicebus.windows.net/;SharedAccessKeyName=trainroot;SharedAccessKey=WWFgnx/Aj+UoLZJDbWybhNWIw2tDvTV815gWb0DBxlI="; } }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SubscriptionId { get; set; }
        public string DataCenterLocation { get; set; }
        public SkuName ServiceBusSkuName { get; set; }
        public SkuTier? ServiceBusSkuTier { get; set; }
    }
    
}
