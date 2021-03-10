using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Infrastructure
{
    public class QueueManager
    {
        private static Lazy<CloudQueueClient> lazyClient = new Lazy<CloudQueueClient>(InitializeQueueClient);
        public static CloudQueueClient queueClient => lazyClient.Value;

        private static CloudQueueClient InitializeQueueClient()
        {
            var account = CloudStorageAccount.Parse(ConfigurationManager.GetValue("GlobalStorage"));
            var tableClient = account.CreateCloudQueueClient();
            return tableClient;
        }
        #region Properties

        public CloudQueue CloudQueue { get; set; }

        #endregion

        #region Constructor

        public QueueManager(string queueName, bool createIfNotExists = false)
        {
            

            CloudQueue = queueClient.GetQueueReference(queueName);//Get cloud queue

            if (createIfNotExists)//Create queue if not exist
                CloudQueue.CreateIfNotExists();
        }

        
        #endregion

        #region Methods

        public bool Exists()
        {
            try
            {
                return CloudQueue.Exists();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void DeleteMessage(CloudQueueMessage cloudQueueMessage)
        {
            try
            {
                CloudQueue.DeleteMessage(cloudQueueMessage);
            }
            catch (Exception)
            {
                // logguer here
            }
        }

        public void DeleteAllMessages()
        {
            try
            {
                CloudQueue.Clear();
            }
            catch (Exception)
            {
                //Logger here
            }
        }

        public CloudQueueMessage GetMessage()
        {
            return CloudQueue.GetMessage();//Get message from queue
        }

        public IEnumerable<CloudQueueMessage> GetMessages(int messagesCount = 32)
        {
            return CloudQueue.GetMessages(messagesCount, new TimeSpan(0, 10, 0));
        }

        public bool Put(string stringContent)
        {
            try
            {
                CloudQueue.AddMessage(new CloudQueueMessage(stringContent));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Put(CloudQueueMessage message)
        {
            try
            {
                CloudQueue.AddMessage(message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Put(byte[] byteContent)
        {
            try
            {
                CloudQueue.AddMessage(new CloudQueueMessage(byteContent));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Put(string stringContent, TimeSpan initialVisibilityDelay)
        {
            try
            {
                CloudQueue.AddMessage(new CloudQueueMessage(stringContent), null, initialVisibilityDelay);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int MessageCount()
        {
            try
            {
                CloudQueue.FetchAttributes();
                return CloudQueue.ApproximateMessageCount ?? 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        #endregion
    }
}
