using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Infrastructure
{
    public static class EventHubHandler
    {
        private static Microsoft.Azure.EventHubs.EventHubClient _eventHubClient;

        private static readonly string EventHubConnectionString = ConfigurationManager.GetValue("EventHubConnectionString");
        public static async Task SendMessagesToEventHub(string eventHubName, dynamic obj)
        {
            try
            {
                if (_eventHubClient == null)
                {
                    CreateClient(eventHubName);
                }

                string json = JsonConvert.SerializeObject(obj);

                await _eventHubClient.SendAsync(new Microsoft.Azure.EventHubs.EventData(Encoding.UTF8.GetBytes(json)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public static void CreateClient(string eventHubName)
        {
            var connectionStringBuilder = new Microsoft.Azure.EventHubs.EventHubsConnectionStringBuilder(EventHubConnectionString)
            {
                EntityPath = eventHubName
            };

            _eventHubClient = Microsoft.Azure.EventHubs.EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
        }
    }
}
