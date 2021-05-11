using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;

using System.Threading.Tasks;

namespace Gosocket.Dian.Infrastructure
{
    public static class StorageTools
    {

        private static Lazy<CloudBlobClient> lazyClient = new Lazy<CloudBlobClient>(InitializeBlobClient);
        public static CloudBlobClient blobClient => lazyClient.Value;

        private static CloudBlobClient InitializeBlobClient()
        {
            var account = CloudStorageAccount.Parse(ConfigurationManager.GetValue("GlobalStorage"));
            var blobClient = account.CreateCloudBlobClient();
            return blobClient;
        }

        public static bool UploadFileFromStream(string blobName, Stream fileStream, string containerName = "provider-files")
        {
            blobName = blobName.ToLower();
            

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            // Create or overwrite the "myblob" blob with contents from a local file.
            blockBlob.UploadFromStream(fileStream);

            return true;
        }

        public static List<CloudBlockBlob> GetBlobFileList(string prefix, string containerName = "provider-files")
        {
            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            var blobs = container.ListBlobs(prefix: prefix, useFlatBlobListing: true);

            List<CloudBlockBlob> result = new List<CloudBlockBlob>();

            foreach (IListBlobItem item in blobs)
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    result.Add(blob);
                }
            }

            return result;
        }

        public static bool UploadFile(string blobName, string text, string containerName = "provider-files")
        {
            blobName = blobName.ToLower();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            // Create or overwrite the "myblob" blob with contents from a local file.
            blockBlob.UploadText(text);


            return true;
        }

        public static string DownloadTextFromFile(string blobName, string containerName = "provider-files")
        {
            try
            {

                blobName = blobName.ToLower();


                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                // Retrieve reference to a blob named "myblob".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                // Create or overwrite the "myblob" blob with contents from a local file.
                var result = blockBlob.DownloadText();


                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static CloudBlockBlob DownloadCloudBlockBlob(string blobName, string containerName = "provider-files")
        {
            try
            {
                blobName = blobName.ToLower();


                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                // Retrieve reference to a blob named "myblob".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                return blockBlob;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static Stream DownloadCloudBlockBlobStream(string blobName, string containerName = "provider-files")
        {
            try
            {
                blobName = blobName.ToLower();
                Stream outPutStream = new MemoryStream();

                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                // Retrieve reference to a blob named "myblob".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                if (blockBlob.Exists())
                {
                    //outPutStream.Position = 0;
                    blockBlob.DownloadToStream(outPutStream);
                    outPutStream.Seek(0, SeekOrigin.Begin);
                }

                return outPutStream;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool UploadText(string text, string blobName, string containerName = "provider-files")
        {
            try
            {

                blobName = blobName.ToLower();


                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                // Retrieve reference to a blob named "myblob".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                // Create or overwrite the "myblob" blob with contents from a local file.
                blockBlob.UploadTextAsync(text).RunSynchronously();


                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool ExistFile(string blobName, string containerName = "provider-files")
        {
            try
            {
                blobName = blobName.ToLower();
                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                // Retrieve reference to a blob named "myblob".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                // Create or overwrite the "myblob" blob with contents from a local file.
                var result = blockBlob.Exists();


                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool DeleteFile(string blobName, string containerName = "provider-files")
        {
            try
            {
                blobName = blobName.ToLower();
                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                // Retrieve reference to a blob named "myblob".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                blockBlob.DeleteIfExists();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static byte[] GetBytes(string blobName, string containerName = "provider-files")
        {
            try
            {
                blobName = blobName.ToLower();
                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                // Retrieve reference to a blob named "myblob".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);


                byte[] bytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    blockBlob.DownloadToStream(ms);
                    bytes = ms.ToArray();
                }
                return bytes;
            }
            catch (StorageException ex)
            {
                _ = ex.Message;
                return null;
            }
        }
    }
}
