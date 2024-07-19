using System.ComponentModel.DataAnnotations;

namespace HoliPics.Models
{
    public class Image
    {
        [Key]
        public int Id { get; set; }

        public int? AlbumId { get; set; }
        public string FileName { get; set; }
    }
}
