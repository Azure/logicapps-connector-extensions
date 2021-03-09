

namespace ServiceProviders.ActiveMQ.Extension
{
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;

    public class ActiveMQConnectionParameters : ConnectionParameters
    {
        public ConnectionStringParameters BrokerUri { get; set; }

        public ConnectionStringParameters ClientId { get; set; }

        public ConnectionStringParameters QueueName { get; set; }

        public ConnectionStringParameters UserName { get; set; }

        public ConnectionStringParameters Password { get; set; }
    }
}