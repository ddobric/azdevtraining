

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using Microsoft.ServiceBus;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Security;
using System.Runtime.InteropServices;

namespace Service
{
    // Tutorial: 
    // https://azure.microsoft.com/en-us/documentation/articles/service-bus-relay-tutorial/
    // Sample with hybrid configuration: 
    // https://azure.microsoft.com/de-de/documentation/articles/service-bus-dotnet-how-to-use-relay/
    class Program
    {
        static void Main(string[] args)
        {
            // create the service host reading the configuration
            ServiceHost host = new ServiceHost(typeof(FileRelayService), new Uri("net.tcp://localhost:9358/fileservice"));

            //ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.AutoDetect;
            //host.AddServiceEndpoint(
            //           typeof(IFileRelayService), new NetTcpBinding(),
            //           "tcp");

            var endpoint = host.AddServiceEndpoint(
               typeof(IFileRelayService), new NetTcpRelayBinding() { IsDynamic = false } ,
               ServiceBusEnvironment.CreateServiceUri("sb", "daenetrelay", "wcf"));

            endpoint.Behaviors.Add(new TransportClientEndpointBehavior
            {
                TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider("sample", "gIE8bsseCJxRkSiOCoNIkiiMBJXW0CygCKXnEvggP9c=")
            });

            //endpoint.Behaviors.Add(
            //  new ServiceRegistrySettings()
            //  {
            //      DisplayName = "FileService Relay2",
            //      DiscoveryMode = DiscoveryType.Public
            //  });


            // open the service
            host.Open();

            Console.ForegroundColor = ConsoleColor.Cyan;
            foreach (ServiceEndpoint ep in host.Description.Endpoints)
            {
                Console.WriteLine("Service address: " + ep.Address.Uri.AbsoluteUri);
            }

            Console.WriteLine("Press [Enter] to exit");

            Console.ReadLine();

            // close the service
            host.Close();
        }


    }
}
