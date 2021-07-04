// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Linq.Expressions;
using System.Runtime.Serialization.Json;
using System.IO;
using System.ServiceModel.Channels;
using System.Xml;
using System.Dynamic;

// https://azure.microsoft.com/en-us/documentation/articles/service-bus-dotnet-how-to-use-relay/

namespace Service
{
    [ServiceBehavior(Name = "EchoService", Namespace = "http://samples.microsoft.com/ServiceModel/Relay/")]
    public class FileRelayService : IFileRelayService
    {
        public string Echo(string text)
        {
            Console.WriteLine("Echoing: {0}", text);
            return text;
        }

        public List<ExpandoObject> Query(string path)
        {
            List<ExpandoObject> items = new List<ExpandoObject>();

            var files = Directory.GetFiles(path, "*.*");

            foreach (var file in files)
            {
                dynamic expandoFile = new ExpandoObject();
                expandoFile.Name = file;
                FileInfo info = new FileInfo(file);
                expandoFile.LastAccessTime = info.LastAccessTime;

                items.Add(expandoFile);
            }

            return items;
        }
    }
}
