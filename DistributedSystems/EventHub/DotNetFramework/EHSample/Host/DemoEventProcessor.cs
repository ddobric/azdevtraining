namespace Host
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using System.Runtime.Serialization.Json;
    using Newtonsoft.Json;

    /// <summary>
    /// The demo event processor.
    /// </summary>
    public class DemoEventProcessor : IEventProcessor
    {
        private int totalMessages = 0;

        private Stopwatch checkpointStopWatch;

        public DemoEventProcessor()
        {
            this.LastMessageOffset = "-1";
        }

        public event EventHandler ProcessorClosed;

        public bool IsInitialized { get; private set; }

        public bool IsClosed { get; private set; }

        public bool IsReceivedMessageAfterClose { get; set; }

        public int TotalMessages
        {
            get
            {
                return this.totalMessages;
            }
        }

        public CloseReason CloseReason { get; private set; }

        public PartitionContext Context { get; private set; }

        public string LastMessageOffset { get; private set; }

        public Task OpenAsync(PartitionContext context)
        {
            // here is the place for you to initialize your processor, e.g. db connection or other stuff. please ensure you have
            // retry or fault handling properly
            Console.WriteLine("{0} > Processor Initializing for PartitionId '{1}' and Owner: {2}.", DateTime.Now.ToString(), context.Lease.PartitionId, context.Lease.Owner ?? string.Empty);
            this.Context = context;
            this.checkpointStopWatch = new Stopwatch();
            this.checkpointStopWatch.Start();
            this.IsInitialized = true;

            return Task.FromResult<object>(null);
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> events)
        {
            // here is the place for you to process the received data for futher processing.
            // suggest you keep it simple fast and reliable.
            try
            {
                foreach (EventData eventData in events)
                {
                    try
                    {
                        var bytes = eventData.GetBytes();
                        string jsonMsg = Encoding.UTF8.GetString(bytes);
                        dynamic msg = JsonConvert.DeserializeObject(jsonMsg);

                        Console.WriteLine($"{msg.Time},  Temperature: {msg.Temperature} at partition {context.Lease.PartitionId}, owner: {context.Lease.Owner}, offset: {eventData.Offset}");

                        // increase the totally amount.
                        Interlocked.Increment(ref this.totalMessages);
                        this.LastMessageOffset = eventData.Offset;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                if (this.IsClosed)
                {
                    this.IsReceivedMessageAfterClose = true;
                }

                if (this.checkpointStopWatch.Elapsed > TimeSpan.FromMinutes(5))
                {
                    lock (this)
                    {
                        this.checkpointStopWatch.Reset();
                        return context.CheckpointAsync();
                    }
                }
            }
            catch (Exception)
            {
                // processor exception handling here.
            }

            return Task.FromResult<object>(null);
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            // where you close the processor.
            Console.WriteLine("{0} > Close called for processor with PartitionId '{1}' and Owner: {2} with reason '{3}'.", DateTime.Now.ToString(), context.Lease.PartitionId, context.Lease.Owner ?? string.Empty, reason);
            this.IsClosed = true;
            this.checkpointStopWatch.Stop();
            this.CloseReason = reason;
            this.OnProcessorClosed();
            return context.CheckpointAsync();
        }

        protected virtual void OnProcessorClosed()
        {
            EventHandler handler = this.ProcessorClosed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
