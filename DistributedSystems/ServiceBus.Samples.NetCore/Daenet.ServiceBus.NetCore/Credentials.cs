using Microsoft.Azure.Management.ServiceBus.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daenet.ServiceBus.NetCore
{
    public class Credentials
    {
        private static string m_SbConnStr = "Endpoint=sb://aztraining.servicebus.windows.net/;SharedAccessKeyName=demo;SharedAccessKey=4+1wNSkzXNbkxgN2T+plmJ8uCuAlAeP6+qm/AjJAa6M=";

        static Credentials()
        {
            if (m_SbConnStr != null)
                return;

            var builder = new ConfigurationBuilder();
            var env = Environment.GetEnvironmentVariable("AzAppConfig");
            if (env != null)
            {
                builder.AddAzureAppConfiguration(env);

                var config = builder.Build();

                var connStr = config["SbConnStr"];
                if (!String.IsNullOrEmpty(connStr))
                    m_SbConnStr = connStr;
            }
            else
            {
                var connStr = Environment.GetEnvironmentVariable("SbConnStr");
                if (connStr != null)
                {
                    m_SbConnStr = connStr;
                }
            }
        }

        public static Credentials Current
        {
            get
            {
                return new Credentials();
            }
        }

        public string ConnStr
        {
            get
            {
                return m_SbConnStr;
            }
        }

        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SubscriptionId { get; set; }
        public string DataCenterLocation { get; set; }
        //public SkuName ServiceBusSkuName { get; set; }
        //public SkuTier? ServiceBusSkuTier { get; set; }
    }

}
