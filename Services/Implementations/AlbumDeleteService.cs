using HoliPics.Authorization;
using HoliPics.Data;
using HoliPics.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoliPics.Services.Implementations
{
    public class AlbumDeleteService : IAlbumDeleteService
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly List<string> _imageSizes;

        public AlbumDeleteService(ApplicationDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
            _imageSizes = new List<string>()
            {
                "Large_", "Medium_", "Small_"
            };
        }

        public async Task<string> DeleteAlbum(int albumId)
        {
            var album = await _context.Albums.FindAsync(albumId);
            
            if (album != null)
            {
                foreach (var filename in album.Images)
                {
                    var image = await _context.Images.FirstOrDefaultAsync(im => im.FileName == filename);

                    if (image != null)
                    {
                        _context.Images.Remove(image);
                    }
                    

                    await _context.SaveChangesAsync();

                    foreach (string size in _imageSizes)
                    {
                        _imageService.DeleteImageFromBlob(size + filename);
                    }
                }
                
                _context.Albums.Remove(album);
            }

            await _context.SaveChangesAsync();
            
            return "";
        }
    }
}
