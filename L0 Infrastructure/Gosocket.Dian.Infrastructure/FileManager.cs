using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Infrastructure
{
    public enum AccessLevel
    {
        Container,
        Blob,
        Private
    }

    public class FileManager
    {
        

        //public CloudBlobClient BlobClient { get; set; }

        private static Lazy<CloudBlobClient> lazyClient = new Lazy<CloudBlobClient>(InitializeBlobClient);
        public static CloudBlobClient BlobClient => lazyClient.Value;

        public CloudBlobClient BlobClientBiller;
        private CloudBlobContainer BlobContainer;
        private string ContainerName;

        private static CloudBlobClient InitializeBlobClient()
        {
            var account = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("GlobalStorage"));
            var blobClient = account.CreateCloudBlobClient();
            return blobClient;
        }

        public FileManager(string container, bool createIfNotExists = false)
        {
            ContainerName = container;
            BlobContainer = BlobClient.GetContainerReference(container);
            if (createIfNotExists)
                BlobContainer.CreateIfNotExists();
        }

        public FileManager(string blobBiller, string container)
        {
            var account = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable(blobBiller));
            var blobClient = account.CreateCloudBlobClient();
            BlobContainer = blobClient.GetContainerReference(container);

        }



        public byte[] GetBytes(string name)
        {
            try
            {
                
                var blob = BlobContainer.GetBlockBlobReference(name);

                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    blob.DownloadToStream(ms);
                    bytes = ms.ToArray();
                }
                return bytes;
            }
            catch (Exception ex)
            {                
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                return null;
            }
        }

        public async Task<byte[]> GetBytesAsync(string name)
        {
            try
            {                
                var blob = BlobContainer.GetBlockBlobReference(name);

                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(ms);
                    bytes = ms.ToArray();
                }
                return bytes;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                return null;
            }
        }

        public byte[] GetBytes(string name, out string contentType)
        {
            try
            {                
                CloudBlockBlob blob = BlobContainer.GetBlockBlobReference(name);

                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    blob.DownloadToStream(ms);
                    bytes = ms.ToArray();
                }
                contentType = blob.Properties.ContentType;
                return bytes;
            }
            catch (StorageException ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                contentType = "";
                return null;
            }
        }

        public Stream GetStream(string name)
        {
            try
            {
                Stream target = new MemoryStream();                
                var blob = BlobContainer.GetBlockBlobReference(name);
                blob.DownloadToStream(target);
                return target;
            }
            catch (StorageException ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                return null;
            }
        }

        public async Task<string> GetTextAsync(string name)
        {
            try
            {                
                var blob = BlobContainer.GetBlockBlobReference(name);

                using (var ms = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(ms);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch (StorageException ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                return null;
            }
        }

        public string GetText(string name)
        {
            try
            {                
                var blob = BlobContainer.GetBlockBlobReference(name);

                string text;
                using (var ms = new MemoryStream())
                {
                    blob.DownloadToStream(ms);
                    text = Encoding.UTF8.GetString(ms.ToArray());
                }
                return text;
            }
            catch (StorageException ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                return null;
            }
        }

        public string GetText(string name, Encoding encoding)
        {
            try
            {             
                var blob = BlobContainer.GetBlockBlobReference(name);

                string text;
                using (var ms = new MemoryStream())
                {
                    blob.DownloadToStream(ms);
                    text = encoding.GetString(ms.ToArray());
                }
                return text;
            }
            catch (StorageException ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                return null;
            }
        }

        public string GetUrl(string name)
        {
            try
            {                
                var blob = BlobContainer.GetBlockBlobReference(name);
                return blob.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                return null;
            }
        }

        public async Task<bool> UploadAsync(string name, byte[] content)
        {
            try
            {                
                var blob = BlobContainer.GetBlockBlobReference(name);                
                using (var ms = new MemoryStream(content))
                {
                    await blob.UploadFromStreamAsync(ms);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                return false;
            }

        }
        public bool Upload(string name, byte[] content)
        {
            try
            {
                
                var blob = BlobContainer.GetBlockBlobReference(name);
                
                using (var ms = new MemoryStream(content))
                {
                    blob.UploadFromStream(ms);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                return false;
            }
        }

        public bool Upload(string name, Stream content,
            string cacheControl = null, AccessLevel accessLevel = AccessLevel.Private)
        {
            try
            {
                
                var blob = BlobContainer.GetBlockBlobReference(name);
                
                blob.UploadFromStream(content);
                if (cacheControl != null)
                {
                    blob.Properties.CacheControl = cacheControl;
                    blob.SetProperties();
                }
                if (accessLevel != AccessLevel.Private)
                    SetContainerACL(BlobContainer, accessLevel.ToString().ToLower());

                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                return false;
            }
        }

        public bool Delete( string name)
        {
            try
            {
                
                var blobReference = BlobContainer.GetBlockBlobReference(name);
                blobReference.Delete();
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                return false;
            }
        }

        public bool DeleteContainer(string container)
        {
            var containerReference = BlobClient.GetContainerReference(container);
            containerReference.Delete();
            return true;
        }

        // ReSharper disable once InconsistentNaming
        private static void SetContainerACL(CloudBlobContainer container, string accessLevel)
        {
            var permissions = new BlobContainerPermissions();
            switch (accessLevel)
            {
                case "container":
                    permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                    break;
                case "blob":
                    permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                    break;
                default:
                    permissions.PublicAccess = BlobContainerPublicAccessType.Off;
                    break;
            }

            container.SetPermissions(permissions);
        }

        public bool Exists(string name)
        {
            try
            {

                var blob = BlobContainer.GetBlockBlobReference(name);
                return blob.Exists();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                return false;
            }
        }

        public bool Exists(string container, string name)
        {
            try
            {
                var blobContainer = BlobClient.GetContainerReference(container);
                var blob = blobContainer.GetBlockBlobReference(name);
                return blob.Exists();                
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                return false;
            }
        }

        public string TryAcquireLease( string name, TimeSpan timeout)
        {
            var content = Encoding.UTF8.GetBytes("content");

            name = name + ".lock";

            if (!Exists(name))
            {
                Upload(name, content);
            }

            
            var blob = BlobContainer.GetBlockBlobReference(name);

            string leaseId = null;

            try
            {
                leaseId = blob.AcquireLease(timeout, null);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                
            }

            return leaseId;
        }

        public bool TryRenewLease(string name, string leaseId)
        {
            name = name + ".lock";

            
            var blob = BlobContainer.GetBlockBlobReference(name);

            try
            {
                blob.RenewLease(AccessCondition.GenerateLeaseCondition(leaseId));
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                return false;
            }
        }

        public void ReleaseLease(string name, string leaseId)
        {
            name = name + ".lock";

            
            var blob = BlobContainer.GetBlockBlobReference(name);

            blob.ReleaseLease(AccessCondition.GenerateLeaseCondition(leaseId));

            var leaseIdContent = Encoding.UTF8.GetBytes("free");
            Upload(name + ".leaseid", leaseIdContent);
        }

        public void BreakLease(string name, string leaseId)
        {
            name = name + ".lock";

            
            var blob = BlobContainer.GetBlockBlobReference(name);

            blob.BreakLease(TimeSpan.MinValue);
        }

        private bool Upload(string container, string name, byte[] content)
        {
            try
            {
                var BlobContainer = BlobClient.GetContainerReference(container);
                var blob = BlobContainer.GetBlockBlobReference(name);
                BlobContainer.CreateIfNotExists();
                using (var ms = new MemoryStream(content))
                {
                    blob.UploadFromStream(ms);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                return false;
            }
        }

        public string TryAcquireLease(TimeSpan? time, string leaseName)
        {
            const string containerName = "lock";
            var fileName = leaseName + ".lock";
            var leaseId = Guid.NewGuid().ToString();

            var content = Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("s"));
            var leaseIdContent = Encoding.UTF8.GetBytes(leaseId);

            if (!Exists(containerName, fileName))
                Upload(containerName, fileName, content);

            var blobContainer = BlobClient.GetContainerReference(containerName);
            var blob = blobContainer.GetBlockBlobReference(fileName);

            try
            {
                blob.AcquireLease(time, leaseId);
                Upload(containerName, fileName + ".leaseid", leaseIdContent);

                return leaseId;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                return null;
            }
        }

        public List<string> GetFileNameList(string ext = ".config")
        {
            
            var result = BlobContainer.ListBlobs(null, true)
                .Where(t => t.Uri.AbsolutePath.ToLower().EndsWith(ext.ToLower()))
                .Select(t => t.Uri.AbsolutePath.Substring(ContainerName.Length + 2)).ToList();
            return result;
        }

        public IEnumerable<IListBlobItem> GetFilesDirectory(string directory)
        {            
            var blobDirectory = BlobContainer.GetDirectoryReference(directory);
            return blobDirectory.ListBlobs();
        }

        public byte[] GetBytesBiller( string name)
        {
            try
            {                
                var blob = BlobContainer.GetBlockBlobReference(name);

                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    blob.DownloadToStream(ms);
                    bytes = ms.ToArray();
                }
                return bytes;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ ex.Message}______{ex.StackTrace}");
                return null;
            }
        }
    }
}