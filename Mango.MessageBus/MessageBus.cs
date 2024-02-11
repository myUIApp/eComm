using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eComm.MessageBus
{
    public class MessageBus : IMessageBus
    {
        private string connectionString = "Endpoint=sb://abhishoppingcart.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=EAbZx4Aefi9h2nKWib9JUxZrtyryfhfhDFDFerer4578898765wsY5jdnp67+ASbEF2bI8=";
        private string connectionStringTopic = "Endpoint=sb://sbfortopics.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=UIW7mqgTVI2Jb4Ttwfhfdghgfu67867571yfpVN56rtysFM7WkBIyc+ASbJ4wa6Y=";
        public async Task PublishMessage(object message, string topic_queue_name)
        {
            await using var client = new ServiceBusClient(connectionString);

            ServiceBusSender sender = client.CreateSender(topic_queue_name);

            var jsonMessage = JsonConvert.SerializeObject(message);
            ServiceBusMessage finalMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
            {
                CorrelationId = Guid.NewGuid().ToString(),
            };

            await sender.SendMessageAsync(finalMessage);
            await client.DisposeAsync();
        }

        // Creating this method because of topic I have created seperate
        public async Task PublishMessageWithTopic(object message, string topic_queue_name)
        {
            await using var client = new ServiceBusClient(connectionStringTopic);

            ServiceBusSender sender = client.CreateSender(topic_queue_name);

            var jsonMessage = JsonConvert.SerializeObject(message);
            ServiceBusMessage finalMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
            {
                CorrelationId = Guid.NewGuid().ToString(),
            };

            await sender.SendMessageAsync(finalMessage);
            await client.DisposeAsync();
        }
    }
}
