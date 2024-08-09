namespace HoliPics.Services.Interfaces
{
    public interface IAlbumDeleteService
    {
        Task<string> DeleteAlbum(int albumId);
    }
}
