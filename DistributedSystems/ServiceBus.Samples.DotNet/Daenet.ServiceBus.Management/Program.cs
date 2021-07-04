using Daenet.ServiceBus.NetCore;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daenet.ServiceBus.Management
{
    class Program
    {
        static void Main(string[] args)
        {
            PrepareQueue("queuesamples/sendreceive");
            //PrepareTopic("topicsamples/sendreceive", false);
        }


        /// <summary>
        /// Prepares an empty queue.
        /// </summary>
        /// <param name="qName">The name/path pf the queue.</param>
        private static QueueDescription PrepareQueue(string qName)
        {
            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(Credentials.Current.ConnStr);

            if (namespaceManager.QueueExists(qName))
                namespaceManager.DeleteQueue(qName);

            QueueDescription qDesc = new QueueDescription(qName);
            qDesc.EnableExpress = false;
            qDesc.DefaultMessageTimeToLive = TimeSpan.FromMinutes(120);
      
            qDesc.RequiresSession = false;
            return qDesc = namespaceManager.CreateQueue(qDesc);
        }


        /// <summary>
        /// Prepares an empty queue with more advanced properties for reliable messaging.
        /// </summary>
        /// <param name="qName">The name/path pf the queue.</param>
        private static QueueDescription PrepareQueue2(string qName)
        {
            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(Credentials.Current.ConnStr);

            if (namespaceManager.QueueExists(qName))
                namespaceManager.DeleteQueue(qName);

            QueueDescription qDesc = new QueueDescription(qName);
            qDesc.EnableDeadLetteringOnMessageExpiration = true;
            qDesc.LockDuration = TimeSpan.FromMinutes(2);
            qDesc.MaxDeliveryCount = 2;
            qDesc.DefaultMessageTimeToLive = TimeSpan.FromMinutes(1);

            qDesc.RequiresSession = false;
            return qDesc = namespaceManager.CreateQueue(qDesc);
        }


        /// <summary>
        /// Prepares an empty queue with more advanced properties for reliable messaging and sessions.
        /// </summary>
        /// <param name="qName">The name/path pf the queue.</param>
        private static QueueDescription PrepareQueue3(string qName)
        {
            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(Credentials.Current.ConnStr);

            if (namespaceManager.QueueExists(qName))
                namespaceManager.DeleteQueue(qName);

            QueueDescription qDesc = new QueueDescription(qName);
            qDesc.EnableDeadLetteringOnMessageExpiration = true;
            qDesc.LockDuration = TimeSpan.FromMinutes(2);
            qDesc.MaxDeliveryCount = 2;
            qDesc.RequiresSession = true;
            qDesc.DefaultMessageTimeToLive = TimeSpan.FromMinutes(1);

            return qDesc = namespaceManager.CreateQueue(qDesc);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="qName"></param>
        /// <returns></returns>
        private static TopicDescription PrepareTopic(string qName, bool requireSession = false)
        {
            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(Credentials.Current.ConnStr);
            if (namespaceManager.TopicExists(qName))
                namespaceManager.DeleteTopic(qName);

            TopicDescription tDesc = new TopicDescription(qName);
         
            tDesc = namespaceManager.CreateTopic(tDesc);
            tDesc.DefaultMessageTimeToLive = TimeSpan.FromMinutes(5);

            var sDesc1 = createSubscription(tDesc.Path, "Subscription1", requireSession, new SqlFilter("Number < 1073741823"));
            var sDesc2 = createSubscription(tDesc.Path, "Subscription2", requireSession, new SqlFilter("Number > 1073741823"));

            return tDesc;
        }

        private static SubscriptionDescription createSubscription(string path,
          string name,
          bool requireSession,
          Filter filter)
        {
            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(Credentials.Current.ConnStr);

            SubscriptionDescription description = new SubscriptionDescription(path, name) { RequiresSession = requireSession };
           
            if (namespaceManager.SubscriptionExists(description.TopicPath, description.Name))
            {
                namespaceManager.DeleteSubscription(description.TopicPath, description.Name);
            }

            description.EnableDeadLetteringOnMessageExpiration = true;
            description.LockDuration = TimeSpan.FromMinutes(2);
            description.MaxDeliveryCount = 2;
            description.RequiresSession = requireSession;

            //sDesc.LockDuration = TimeSpan.FromDays(2);
            return namespaceManager.CreateSubscription(description, filter);
        }
    }
}
