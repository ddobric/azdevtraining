
using Azure.Messaging.ServiceBus.Administration;
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
        public static async Task<QueueProperties> EnsureQueueExists(string queueName, bool requireSession = false)
        {
            ServiceBusAdministrationClient mgmClient = new ServiceBusAdministrationClient(Credentials.Current.ConnStr);

            QueueProperties qProps;

            try
            {
                var options = new CreateQueueOptions(queueName)
                {
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
                    MaxDeliveryCount = 7,

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
        public static async Task<TopicProperties> EnsureTopicExists(string topicName, string sub1, string sub2, bool requireSession = false)
        {
            ServiceBusAdministrationClient mgmClient = new ServiceBusAdministrationClient(Credentials.Current.ConnStr);
            TopicProperties tProps;

            var options = new CreateTopicOptions(topicName)
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

            if (mgmClient.TopicExistsAsync(topicName).Result)
            {
                await mgmClient.DeleteTopicAsync(topicName);
            }

            tProps = await mgmClient.CreateTopicAsync(options);

            await CreateSubscriptions(topicName, sub1, sub2, requireSession, true);

            return tProps;
        }


        private static async Task CreateSubscriptions(string topicName,  string s1Name, string s2Name, bool requireSession, bool createRules = false)
        {
            ServiceBusAdministrationClient mgmClient = new ServiceBusAdministrationClient(Credentials.Current.ConnStr);
          
            var subOpts1= new CreateSubscriptionOptions(topicName, s1Name)
            {
                AutoDeleteOnIdle = TimeSpan.FromDays(7),
                DefaultMessageTimeToLive = TimeSpan.FromDays(2),
                EnableBatchedOperations = true,
                UserMetadata = "some metadata",
                RequiresSession = requireSession
            };

            var subOpts2 = new CreateSubscriptionOptions(topicName, s2Name)
            {
                AutoDeleteOnIdle = TimeSpan.FromDays(7),
                DefaultMessageTimeToLive = TimeSpan.FromDays(2),
                EnableBatchedOperations = true,
                UserMetadata = "some metadata",
                RequiresSession = requireSession
            };


            if (await mgmClient.SubscriptionExistsAsync(topicName, s1Name))
            {
                await mgmClient.DeleteSubscriptionAsync(topicName, s1Name);
            }


            if (await mgmClient.SubscriptionExistsAsync(topicName, s2Name))
            {
                await mgmClient.DeleteSubscriptionAsync(topicName, s2Name);
            }

            await mgmClient.CreateSubscriptionAsync(subOpts1);

            await mgmClient.CreateSubscriptionAsync(subOpts2);
                     
            if (createRules)
            {
                CreateRuleOptions ruleOpts1 = new CreateRuleOptions
                {
                    Name = "LessThan",
                    Filter = new SqlRuleFilter($"Num<{int.MaxValue / 2}"),
                };

                await mgmClient.CreateRuleAsync(topicName, s1Name, ruleOpts1);

                CreateRuleOptions ruleOpts2 = new CreateRuleOptions
                {
                    Name = "GreaterThan",
                    Filter = new SqlRuleFilter($"Num>={int.MaxValue / 2}"),
                };

                await mgmClient.CreateRuleAsync(topicName, s2Name, ruleOpts2);
            }
        }
    }
}
