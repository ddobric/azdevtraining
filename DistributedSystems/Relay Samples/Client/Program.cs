

using Microsoft.ServiceBus;
using Service;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;

namespace Client
{
    class Program
    {
        static bool validateServerCert(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }

        static void Main(string[] args)
        {
            ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.AutoDetect;

            var channelFactory = new ChannelFactory<IFileRelayService>( new NetTcpRelayBinding(),
                new EndpointAddress(ServiceBusEnvironment.CreateServiceUri("sb", "daenetrelay", "wcf")));
            
            channelFactory.Endpoint.Behaviors.Add(new TransportClientEndpointBehavior
            { TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider("sample", "gIE8bsseCJxRkSiOCoNIkiiMBJXW0CygCKXnEvggP9c=") });

            var channel = channelFactory.CreateChannel();
            
            while (true)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Enter path.");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    string filePath = Console.ReadLine();

                    if (filePath == String.Empty)
                        break;

                    Console.WriteLine("Server echoed: {0}", channel.Echo(filePath));

                    List<ExpandoObject> res = channel.Query(filePath);

                    foreach (ExpandoObject item in res)
                    {
                        dynamic a = item;
                        Console.WriteLine($"Name {a.Name}, Last Access Time: {a.LastAccessTime}");
                    }
                                       
                    Console.WriteLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                    channel = channelFactory.CreateChannel();
                    ((ICommunicationObject)channel).Open();
                }
            }

            ((ICommunicationObject)channel).Close();
            channelFactory.Close();
        }
    }
}
