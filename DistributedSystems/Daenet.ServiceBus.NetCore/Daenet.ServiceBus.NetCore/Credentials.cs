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
        private static string m_SbConnStr = "Endpoint=sb://aaaaaaaaaaaaaa.servicebus.windows.net/;SharedAccessKeyName=demo;SharedAccessKey=NEW0MqWB10ZNN1I5XfL1lA/iqmJ9ENL7ZXTbEaPA5P8=";

        static Credentials()
        {
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

        // public string ConnStr { get  { return "Endpoint=sb://students.servicebus.windows.net/;SharedAccessKeyName=stud-2018;SharedAccessKey=AQiFJiPtD0G/7y8hXStqt8CXZR+M1LSzOfGPiEoL0cc="; } }
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
        public SkuName ServiceBusSkuName { get; set; }
        public SkuTier? ServiceBusSkuTier { get; set; }
    }

}
