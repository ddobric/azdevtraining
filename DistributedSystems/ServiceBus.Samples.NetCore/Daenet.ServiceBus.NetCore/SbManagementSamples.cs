
using Azure.Messaging.ServiceBus.Administration;
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
        public static async Task<QueueProperties> CreateQueue(string queueName, bool requireSession = false)
        {
            ServiceBusAdministrationClient mgmClient = new ServiceBusAdministrationClient(Credentials.Current.ConnStr);

            QueueProperties qProps;

            try
            {
                var options = new CreateQueueOptions(queueName)
                {
                    // Duration of idle interval after which the queue is automatically deleted. 
                    AutoDeleteOnIdle = TimeSpan.FromDays(7),

                    DefaultMessageTimeToLive = TimeSpan.FromDays(2),
                    DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(1),
                    EnableBatchedOperations = true,
                    DeadLetteringOnMessageExpiration = true,
                    EnablePartitioning = false,
                    ForwardDeadLetteredMessagesTo = null,
                    ForwardTo = null,

                    // The duration of a peek lock; that is, the amount of time that a message is locked from other receivers.
                    LockDuration = TimeSpan.FromSeconds(45),
                    MaxDeliveryCount = 8,

                    // Size of the Queue. For non-partitioned entity, this would be the size of the queue. 
                    // For partitioned entity, this would be the size of each partition.
                    MaxSizeInMegabytes = 2048,

                    RequiresDuplicateDetection = true,
                    RequiresSession = requireSession,
                    UserMetadata = "some metadata"
                };

                if (mgmClient.QueueExistsAsync(queueName).Result == false)
                {
                    qProps = await mgmClient.CreateQueueAsync(options);
                }
                else
                {
                    //queueDescription = await mgmClient.GetQueueAsync(qName);
                    await mgmClient.DeleteQueueAsync(queueName);
                    qProps = await mgmClient.CreateQueueAsync(options);
                }

                return qProps;
            }
            catch (AggregateException ex)
            {
                Console.WriteLine($"{ex}");
                throw;
            }
        }


        /// <summary>
        /// Demonstrates how to create a new queue or to return existing one.
        /// </summary>
        /// <param name="qName">The name/path pf the queue.</param>
        public static async Task<TopicProperties> CreateTopic(string queueName, bool requireSession = false)
        {
            ServiceBusAdministrationClient mgmClient = new ServiceBusAdministrationClient(Credentials.Current.ConnStr);
            TopicProperties tProps;

            var options = new CreateTopicOptions(queueName)
            {
                // Duration of idle interval after which the queue is automatically deleted. 
                AutoDeleteOnIdle = TimeSpan.FromDays(7),

                DefaultMessageTimeToLive = TimeSpan.FromDays(2),
                DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(1),
                EnableBatchedOperations = true,
                EnablePartitioning = false,

                // Size of the Queue. For non-partitioned entity, this would be the size of the queue. 
                // For partitioned entity, this would be the size of each partition.
                MaxSizeInMegabytes = 2048,

                RequiresDuplicateDetection = true,
                UserMetadata = "some metadata"
            };

            if (mgmClient.TopicExistsAsync(queueName).Result)
            {
                await mgmClient.DeleteTopicAsync(queueName);
            }

            tProps = await mgmClient.CreateTopicAsync(options);

            return tProps;

        }



        private static async Task<SubscriptionProperties> CreateSubscription(string topicName, string path, string sName, bool requireSession, bool createRules = false)
        {
            ServiceBusAdministrationClient mgmClient = new ServiceBusAdministrationClient(Credentials.Current.ConnStr);
          
            var options = new CreateSubscriptionOptions(topicName, sName)
            {
                AutoDeleteOnIdle = TimeSpan.FromDays(7),
                DefaultMessageTimeToLive = TimeSpan.FromDays(2),
                EnableBatchedOperations = true,
                UserMetadata = "some metadata",
                RequiresSession = requireSession
            };


            if (await mgmClient.SubscriptionExistsAsync(topicName, sName))
            {
                await mgmClient.DeleteSubscriptionAsync(topicName, sName);
            }

            SubscriptionProperties subProps = await mgmClient.CreateSubscriptionAsync(options);
            if (createRules)
            {
                CreateRuleOptions ruleOpts1 = new CreateRuleOptions
                {
                    Name = "LessThan",
                    Filter = new SqlRuleFilter($"Num<{int.MaxValue / 2}"),
                };

                await mgmClient.CreateRuleAsync(topicName, sName, ruleOpts1);

                CreateRuleOptions ruleOpts2 = new CreateRuleOptions
                {
                    Name = "GreaterThan",
                    Filter = new SqlRuleFilter($"Num>={int.MaxValue / 2}"),
                };
            }

            return subProps;
        }
    }
}
