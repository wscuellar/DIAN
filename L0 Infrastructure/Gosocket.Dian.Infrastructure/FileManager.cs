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
        private static FileManager _instance = null;

        //public CloudBlobClient BlobClient { get; set; }

        private static Lazy<CloudBlobClient> lazyClient = new Lazy<CloudBlobClient>(InitializeBlobClient);
        public static CloudBlobClient BlobClient => lazyClient.Value;

        public CloudBlobClient BlobClientBiller;

        private static CloudBlobClient InitializeBlobClient()
        {
            var account = CloudStorageAccount.Parse(ConfigurationManager.GetValue("GlobalStorage"));
            var blobClient = account.CreateCloudBlobClient();
            return blobClient;
        }

        public FileManager()
        {
            
        }

        public FileManager(string blobBiller)
        {
            var account = CloudStorageAccount.Parse(ConfigurationManager.GetValue(blobBiller));
            var blobClient = account.CreateCloudBlobClient();
            BlobClientBiller = blobClient;
        }

        public static FileManager Instance => _instance ?? (_instance = new FileManager());

        public byte[] GetBytes(string container, string name)
        {
            try
            {
                var blobContainer = BlobClient.GetContainerReference(container);
                var blob = blobContainer.GetBlockBlobReference(name);

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
                _ = ex.Message;
                return null;
            }
        }

        public async Task<byte[]> GetBytesAsync(string container, string name)
        {
            try
            {
                var blobContainer = BlobClient.GetContainerReference(container);
                var blob = blobContainer.GetBlockBlobReference(name);

                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(ms);
                    bytes = ms.ToArray();
                }
                return bytes;
            }
            catch
            {
                return null;
            }
        }

        public byte[] GetBytes(string container, string name, out string contentType)
        {
            try
            {
                var blobContainer = BlobClient.GetContainerReference(container);
                CloudBlockBlob blob = blobContainer.GetBlockBlobReference(name);

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
                Console.WriteLine(ex.Message);
                Console.Out.WriteLine(ex.Message);
                contentType = "";
                return null;
            }
        }

        public Stream GetStream(string container, string name)
        {
            try
            {
                Stream target = new MemoryStream();
                var blobContainer = BlobClient.GetContainerReference(container);
                var blob = blobContainer.GetBlockBlobReference(name);
                blob.DownloadToStream(target);
                return target;
            }
            catch (StorageException)
            {
                return null;
            }
        }

        public async Task<string> GetTextAsync(string container, string name)
        {
            try
            {
                var blobContainer = BlobClient.GetContainerReference(container);
                var blob = blobContainer.GetBlockBlobReference(name);

                using (var ms = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(ms);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch (StorageException)
            {
                return null;
            }
        }

        public string GetText(string container, string name)
        {
            try
            {
                var blobContainer = BlobClient.GetContainerReference(container);
                var blob = blobContainer.GetBlockBlobReference(name);

                string text;
                using (var ms = new MemoryStream())
                {
                    blob.DownloadToStream(ms);
                    text = Encoding.UTF8.GetString(ms.ToArray());
                }
                return text;
            }
            catch (StorageException)
            {
                return null;
            }
        }

        public string GetText(string container, string name, Encoding encoding)
        {
            try
            {
                var blobContainer = BlobClient.GetContainerReference(container);
                var blob = blobContainer.GetBlockBlobReference(name);

                string text;
                using (var ms = new MemoryStream())
                {
                    blob.DownloadToStream(ms);
                    text = encoding.GetString(ms.ToArray());
                }
                return text;
            }
            catch (StorageException)
            {
                return null;
            }
        }

        public string GetUrl(string container, string name)
        {
            try
            {
                var blobContainer = BlobClient.GetContainerReference(container);
                var blob = blobContainer.GetBlockBlobReference(name);
                return blob.Uri.AbsoluteUri;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> UploadAsync(string container, string name, byte[] content)
        {
            try
            {
                var blobContainer = BlobClient.GetContainerReference(container);
                var blob = blobContainer.GetBlockBlobReference(name);
                blobContainer.CreateIfNotExists();
                using (var ms = new MemoryStream(content))
                {
                    await blob.UploadFromStreamAsync(ms);
                }
                return true;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return false;
            }

        }
        public bool Upload(string container, string name, byte[] content)
        {
            try
            {
                var blobContainer = BlobClient.GetContainerReference(container);
                var blob = blobContainer.GetBlockBlobReference(name);
                blobContainer.CreateIfNotExists();
                using (var ms = new MemoryStream(content))
                {
                    blob.UploadFromStream(ms);
                }
                return true;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return false;
            }
        }

        public bool Upload(string container, string name, Stream content,
            string cacheControl = null, AccessLevel accessLevel = AccessLevel.Private)
        {
            try
            {
                var blobContainer = BlobClient.GetContainerReference(container);
                var blob = blobContainer.GetBlockBlobReference(name);
                blobContainer.CreateIfNotExists();
                blob.UploadFromStream(content);
                if (cacheControl != null)
                {
                    blob.Properties.CacheControl = cacheControl;
                    blob.SetProperties();
                }
                if (accessLevel != AccessLevel.Private)
                    SetContainerACL(blobContainer, accessLevel.ToString().ToLower());

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Delete(string container, string name)
        {
            try
            {
                var containerReference = BlobClient.GetContainerReference(container);
                var blobReference = containerReference.GetBlockBlobReference(name);
                blobReference.Delete();
                return true;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
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

        public bool Exists(string container, string name)
        {
            try
            {
                var blobContainer = BlobClient.GetContainerReference(container);
                var blob = blobContainer.GetBlockBlobReference(name);
                return blob.Exists();                
            }
            catch
            {
                return false;
            }
        }

        public string TryAcquireLease(string container, string name, TimeSpan timeout)
        {
            var content = Encoding.UTF8.GetBytes("content");

            name = name + ".lock";

            if (!Exists(container, name))
            {
                Upload(container, name, content);
            }

            var blobContainer = BlobClient.GetContainerReference(container);
            var blob = blobContainer.GetBlockBlobReference(name);

            string leaseId = null;

            try
            {
                leaseId = blob.AcquireLease(timeout, null);
            }
            catch (Exception)
            {
                // ignored
            }

            return leaseId;
        }

        public bool TryRenewLease(string container, string name, string leaseId)
        {
            name = name + ".lock";

            var blobContainer = BlobClient.GetContainerReference(container);
            var blob = blobContainer.GetBlockBlobReference(name);

            try
            {
                blob.RenewLease(AccessCondition.GenerateLeaseCondition(leaseId));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ReleaseLease(string container, string name, string leaseId)
        {
            name = name + ".lock";

            var blobContainer = BlobClient.GetContainerReference(container);
            var blob = blobContainer.GetBlockBlobReference(name);

            blob.ReleaseLease(AccessCondition.GenerateLeaseCondition(leaseId));

            var leaseIdContent = Encoding.UTF8.GetBytes("free");
            Upload(container, name + ".leaseid", leaseIdContent);
        }

        public void BreakLease(string container, string name, string leaseId)
        {
            name = name + ".lock";

            var blobContainer = BlobClient.GetContainerReference(container);
            var blob = blobContainer.GetBlockBlobReference(name);

            blob.BreakLease(TimeSpan.MinValue);
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
            catch (Exception)
            {
                return null;
            }
        }

        public List<string> GetFileNameList(string container, string ext = ".config")
        {
            var blobContainer = BlobClient.GetContainerReference(container);
            var result = blobContainer.ListBlobs(null, true)
                .Where(t => t.Uri.AbsolutePath.ToLower().EndsWith(ext.ToLower()))
                .Select(t => t.Uri.AbsolutePath.Substring(container.Length + 2)).ToList();
            return result;
        }

        public IEnumerable<IListBlobItem> GetFilesDirectory(string container, string directory)
        {
            var blobContainer = BlobClient.GetContainerReference(container);
            var blobDirectory = blobContainer.GetDirectoryReference(directory);
            return blobDirectory.ListBlobs();
        }

        public byte[] GetBytesBiller(string container, string name)
        {
            try
            {
                var blobContainer = BlobClientBiller.GetContainerReference(container);
                var blob = blobContainer.GetBlockBlobReference(name);

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
                _ = ex.Message;
                return null;
            }
        }
    }
}