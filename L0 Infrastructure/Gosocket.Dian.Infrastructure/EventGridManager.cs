using Microsoft.Azure;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Infrastructure
{
    public class EventGridManager
    {
        static readonly object lockInstance = new object();

        private static Dictionary<string, EventGridManager> instance = new Dictionary<string, EventGridManager>();
        private static Dictionary<string, EventGridClient> clients = new Dictionary<string, EventGridClient>();

        private static string _eventGridKey;
        private static string _eventGridTopicEndpoint;

        public static EventGridManager Instance(string EventGridKey = "EventGridKey", string EventGridTopicEndpoint = "EventGridTopicEndpoint")
        {
            string topicEndpoint = CloudConfigurationManager.GetSetting(EventGridTopicEndpoint);
            string topicKey = CloudConfigurationManager.GetSetting(EventGridKey);

            _eventGridKey = topicKey;
            _eventGridTopicEndpoint = topicEndpoint;

            if (!instance.ContainsKey(topicKey + "|" + topicEndpoint))
            {
                lock (lockInstance)
                {

                    TopicCredentials topicCredentials = new TopicCredentials(topicKey);
                    EventGridClient client = new EventGridClient(topicCredentials);
                    instance[topicKey + "|" + topicEndpoint] = new EventGridManager();
                    clients[topicKey + "|" + topicEndpoint] = client;
                }
            }

            return instance[topicKey + "|" + topicEndpoint];
        }

        public async Task SendMessagesToEventGridAsync(List<EventGridEvent> events)
        {
            try
            {
                string topicHostname = new Uri(_eventGridTopicEndpoint).Host;
                var result = await clients[_eventGridKey + "|" + _eventGridTopicEndpoint].PublishEventsWithHttpMessagesAsync(topicHostname, events);

                if (result.Response.IsSuccessStatusCode)
                    return;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void SendMessagesToEventGrid(List<EventGridEvent> events)
        {
            try
            {
                string topicHostname = new Uri(_eventGridTopicEndpoint).Host;
                var result = clients[_eventGridKey + "|" + _eventGridTopicEndpoint].PublishEventsWithHttpMessagesAsync(topicHostname, events).GetAwaiter().GetResult();

                if (result.Response.IsSuccessStatusCode)
                    return;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
