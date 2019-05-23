using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

/*
 * Developer name: Gene Joseph Garcia
 * Description: This class will handle all the blob uploading, deleting, and retrieving from the database.
 * 
 * */

namespace MCLScholarWeb.Storage
{
    public class AzureBlobStorage
    {

        public static string CONTAINER_ANNOUNCEMENT = "announcementresources";
        public static string CONTAINER_FORMS = "formsresources";
        public static string CONTAINER_REQUIREMENTS = "requestsresources";

        private StorageCredentials credentials;
        private CloudStorageAccount storageAccount;
        private CloudBlobClient client;
        private CloudBlobContainer container;

        private string downloadPath = "C:\\MCLScholarWebsiteDownloads\\";
        private string containerName;
        private string accountName = "mclscholarres";   

        public AzureBlobStorage(string containerName)
        {
            this.containerName = containerName;
            string key = "<<Truncated>>";

            credentials = new StorageCredentials(accountName, key);
            storageAccount = new CloudStorageAccount(credentials, true);
            client = storageAccount.CreateCloudBlobClient();
            container = client.GetContainerReference(containerName);
        }

        /*
         * Uploads the file, photos or pdfs in the case of this website, to the azure blob storage.
         * */
        public bool UploadBlob(HttpPostedFileBase file)
        {
            if (file == null)
                return false;
            string fileName = Path.GetFileName(file.FileName);

            container = client.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            if (blockBlob.Exists())
            {
                return false;
            }

            using (var fileStream = (file.InputStream))
            {
                blockBlob.UploadFromStream(fileStream);
            }

            return true;
        }

        /*
         * In uploading blobs in azure blob storage, uploading similiar file names will only overwrite the existing file. Thus, could create errors if the blob
         * is deleted. So this upload similar blob will be renaming the file name and then uploading to the database.
         * */
        public string UploadBlobSimilarBlob(HttpPostedFileBase file, int highestFileID)
        {
            container = client.GetContainerReference(containerName);

            string fileNameWithLocation = Path.GetFileName(file.FileName);
            string fileName = fileNameWithLocation.Substring(0, fileNameWithLocation.LastIndexOf('.')) + "-" + highestFileID + System.IO.Path.GetExtension(fileNameWithLocation);

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            using (var fileStream = (file.InputStream))
            {
                blockBlob.UploadFromStream(fileStream);
            }

            return fileName;
        }

        /*
         * This will return a list of blob model.
         * */
        public IEnumerable<BlobModel> GetBlobs()
        {
            var itemList = container.ListBlobs().ToList();

            IEnumerable<BlobModel> blobList = itemList.Select(
                blobModel => new BlobModel
                {
                    BlobContainerName = blobModel.Container.Name,
                    FileURI = blobModel.Uri.ToString(),
                    FileName = blobModel.Uri.AbsoluteUri.Substring(blobModel.Uri.AbsoluteUri.LastIndexOf("/") + 1)
                }
                ).ToList();

            return blobList;
        }

        public bool DeleteBlob(string fileName)
        {
            
            container = client.GetContainerReference(containerName);

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            bool isDeleted = blockBlob.DeleteIfExists();

            return isDeleted;

        }

        public CloudBlob GetCloudBlobByFileReferece(string fileName)
        {
            //container = client.GetContainerReference(containerName);
            var blob = container.GetBlockBlobReference(fileName);
            
            return blob;
        }
    }
}
