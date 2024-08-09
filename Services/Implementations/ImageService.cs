using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using HoliPics.Options;
using HoliPics.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace HoliPics.Services.Implementations
{
    public class ImageService : IImageService
    {

        private readonly AzureBlobOptions _azureBlobOptions;

        public ImageService(IOptions<AzureBlobOptions> azureBlobOptions)
        {
            _azureBlobOptions = azureBlobOptions.Value;
        }

        public void UploadImageToBlob(Stream imageStream, string uniqueName)
        {
            using MemoryStream uploadStream = new MemoryStream();

            BlobContainerClient blobContainerClient = new BlobContainerClient(
                _azureBlobOptions.ConnectionString,
                _azureBlobOptions.Container);
            BlobClient blobClient = blobContainerClient.GetBlobClient(uniqueName);
            imageStream.Position = 0;
            blobClient.Upload(imageStream, new BlobUploadOptions()
            {
                HttpHeaders = new BlobHttpHeaders()
                {
                    ContentType = "image/bitmap"
                }
            }, cancellationToken: default);            
        }

        public async void DeleteImageFromBlob(string uniqueName)
        {
            BlobContainerClient blobContainerClient = new BlobContainerClient(
                _azureBlobOptions.ConnectionString,
                _azureBlobOptions.Container);
            BlobClient blobClient = blobContainerClient.GetBlobClient(uniqueName);
            await blobClient.DeleteAsync(snapshotsOption: DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: default);
        }

        public async Task<(Stream, string)> GetImageFromBlob(string uniqueName)
        {
            BlobContainerClient blobContainerClient = new BlobContainerClient(
                _azureBlobOptions.ConnectionString,
                _azureBlobOptions.Container);
            BlobClient blobClient = blobContainerClient.GetBlobClient(uniqueName);
           
            BlobProperties blobProperties = blobClient.GetProperties();
            
            MemoryStream imageStream = new MemoryStream();
            await blobClient.DownloadToAsync(imageStream);
            imageStream.Position = 0;
            return (imageStream, blobProperties.ContentType);
        }        
    }
}

