
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Linq.Expressions;
using System.Dynamic;

namespace Service
{
    [ServiceContract(Name = "EchoContract", Namespace = "http://samples.microsoft.com/ServiceModel/Relay/")]
    public interface IFileRelayService
    {
        [OperationContract]
        string Echo(string text);

        [OperationContract]
        List<ExpandoObject> Query(string expression);


    }
}
