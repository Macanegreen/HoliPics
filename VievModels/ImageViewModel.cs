using HoliPics.Models.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HoliPics.VievModels
{
    public class ImageViewModel
    {
        public int AlbumId { get; set; }
        [Required(ErrorMessage = "Please select a picture to upload.")]
        [AllowedExtensions(new string[] { ".gif", ".jpg", ".png" })]
        public List<IFormFile> ImageFiles { get; set; }       
    }

    
}
