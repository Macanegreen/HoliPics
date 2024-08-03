namespace HoliPics.Services.Interfaces
{
    public interface IImageService
    {
        void UploadImageToBlob(Stream imageStream, string uniqueName);
        void DeleteImageFromBlob(string uniqueName);
        Task<(Stream, string)> GetImageFromBlob(string uniqueName);
    }
}
