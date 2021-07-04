using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Daenet.ServiceBus.NetCore
{
    public static class SbManagementSamples
    {
        /// <summary>
        /// Demonstrates how to create a new queue or to return existing one.
        /// </summary>
        /// <param name="qName">The name/path pf the queue.</param>
        public static async Task<QueueDescription> PrepareQueue(string qName, bool requireSession = false)
        {
            ManagementClient mgmClient = new ManagementClient(Credentials.Current.ConnStr);

            try
            {
                QueueDescription queueDescription;

                if (mgmClient.QueueExistsAsync(qName).Result == false)
                {
                    queueDescription = await createQueue(qName, requireSession, mgmClient);
                }
                else
                {
                    //queueDescription = await mgmClient.GetQueueAsync(qName);
                    await mgmClient.DeleteQueueAsync(qName);
                    queueDescription = await createQueue(qName, requireSession, mgmClient);
                }

                return queueDescription;

            }
            catch (AggregateException ex)
            {
                switch (ex.InnerException)
                {
                    case MessagingEntityNotFoundException notFoundEx:

                        Console.WriteLine($"Queue does not exist: {qName}");
                        throw;

                    case MessagingEntityAlreadyExistsException alreadyExistEx:
                        Console.WriteLine($"{ex}");
                        throw;

                    case MessagingEntityDisabledException disabledEx:
                        Console.WriteLine($"{ex}");
                        throw;
                    case ServiceBusException sbEx:
                        Console.WriteLine($"{ex}");
                        throw;

                    default:
                        throw;
                }
            }
        }

        private static async Task<QueueDescription> createQueue(string qName, bool requireSession, ManagementClient mgmClient)
        {
            QueueDescription queueDescription;
            queueDescription = new QueueDescription(qName)
            {
                // The duration of a peek lock; that is, the amount of time that a message is locked from other receivers.
                LockDuration = TimeSpan.FromSeconds(45),

                // Size of the Queue. For non-partitioned entity, this would be the size of the queue. 
                // For partitioned entity, this would be the size of each partition.
                MaxSizeInMB = 2048,

                // This value indicates if the queue requires guard against duplicate messages. 
                // Find out more in DuplicateDetection sample
                RequiresDuplicateDetection = false,

                //Since RequiresDuplicateDetection is false, the following need not be specified and will be ignored.
                //DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(2),

                // This indicates whether the queue supports the concept of session.
                // Find out more in "Session and Workflow Management Features" sample
                RequiresSession = requireSession,

                // The default time to live value for the messages
                // Find out more in "TimeToLive" sample.
                DefaultMessageTimeToLive = TimeSpan.FromMinutes(5),

                // Duration of idle interval after which the queue is automatically deleted. 
                AutoDeleteOnIdle = TimeSpan.MaxValue,

                // Decides whether an expired message due to TTL should be dead-letterd
                // Find out more in "TimeToLive" sample.
                EnableDeadLetteringOnMessageExpiration = false,

                // The maximum delivery count of a message before it is dead-lettered
                // Find out more in "DeadletterQueue" sample
                MaxDeliveryCount = 8,

                // Creating only one partition. 
                // Find out more in PartitionedQueues sample.
                EnablePartitioning = false
            };

            queueDescription = await mgmClient.CreateQueueAsync(queueDescription);
            return queueDescription;
        }


        /// <summary>
        /// Demonstrates how to create a new queue or to return existing one.
        /// </summary>
        /// <param name="qName">The name/path pf the queue.</param>
        public static async Task<TopicDescription> PrepareTopic(string qName, bool requireSession = false)
        {
            ManagementClient mgmClient = new ManagementClient(Credentials.Current.ConnStr);

            try
            {
                TopicDescription topicDescription;

                if (mgmClient.TopicExistsAsync(qName).Result == false)
                {
                    topicDescription = await createTopic(qName, requireSession, mgmClient);
                }
                else
                {
                    //queueDescription = await mgmClient.GetQueueAsync(qName);
                    await mgmClient.DeleteTopicAsync(qName);
                    topicDescription = await createTopic(qName, requireSession, mgmClient);
                }

                return topicDescription;

            }
            catch (AggregateException ex)
            {
                switch (ex.InnerException)
                {
                    case MessagingEntityNotFoundException notFoundEx:

                        Console.WriteLine($"Queue does not exist: {qName}");
                        throw;

                    case MessagingEntityAlreadyExistsException alreadyExistEx:
                        Console.WriteLine($"{ex}");
                        throw;

                    case MessagingEntityDisabledException disabledEx:
                        Console.WriteLine($"{ex}");
                        throw;
                    case ServiceBusException sbEx:
                        Console.WriteLine($"{ex}");
                        throw;

                    default:
                        throw;
                }
            }
        }


        private static async Task<TopicDescription> createTopic(string qName, bool requireSession, ManagementClient mgmClient)
        {
            TopicDescription topicDescription;
            topicDescription = new TopicDescription(qName)
            {

                // Size of the Queue. For non-partitioned entity, this would be the size of the queue. 
                // For partitioned entity, this would be the size of each partition.
                MaxSizeInMB = 2048,

                // This value indicates if the queue requires guard against duplicate messages. 
                // Find out more in DuplicateDetection sample
                RequiresDuplicateDetection = false,

                //Since RequiresDuplicateDetection is false, the following need not be specified and will be ignored.
                //DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(2),


                // The default time to live value for the messages
                // Find out more in "TimeToLive" sample.
                DefaultMessageTimeToLive = TimeSpan.FromMinutes(5),

                // Duration of idle interval after which the queue is automatically deleted. 
                AutoDeleteOnIdle = TimeSpan.MaxValue,

                // Creating only one partition. 
                // Find out more in PartitionedQueues sample.
                EnablePartitioning = false
            };

            topicDescription = await mgmClient.CreateTopicAsync(topicDescription);

            await createSubscription(mgmClient, topicDescription.Path, TopicSample.cSubscriptionName1, requireSession);

            await createSubscription(mgmClient, topicDescription.Path, TopicSample.cSubscriptionName2, requireSession);

            return topicDescription;
        }

        private static async Task<SubscriptionDescription> createSubscription(ManagementClient mgmClient,
            string path,
          string name,
          bool requireSession,
          Filter filter = null)
        {
          
            SubscriptionDescription description = new SubscriptionDescription(path, name)
            {
                RequiresSession = requireSession
            };
           
            if (await mgmClient.SubscriptionExistsAsync(path, name))
            {
                await mgmClient.DeleteSubscriptionAsync(path, name);
            }

            //sDesc.LockDuration = TimeSpan.FromDays(2);
            return await mgmClient.CreateSubscriptionAsync(description);
        }

        private static async Task<RuleDescription> createRule(ManagementClient mgmClient,
          string topicPath,
        string subscriptionName,       
        Filter filter)
        {
            if (await mgmClient.SubscriptionExistsAsync(topicPath, subscriptionName))
            {
                await mgmClient.DeleteSubscriptionAsync(topicPath, subscriptionName);
            }

            RuleDescription rule = new RuleDescription()
            {
                 Filter = filter,
            };

            return await mgmClient.CreateRuleAsync(topicPath, subscriptionName, rule);
        }
    }
}
