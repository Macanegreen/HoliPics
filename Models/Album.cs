

using System.ComponentModel.DataAnnotations;

namespace HoliPics.Models
{
    public class Album
    {
        public int Id { get; set; }
        public string? CreatorId { get; set; }
        [Display(Name = "Created by")]
        public string? CreatedBy { get; set; }
        [Display(Name = "Time of creation")]
        public DateTime CreationTime { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public List<string> Images { get; set; }   
        public int UploadProgress { get; set; }
        public string? Thumbnail { get; set; }
        
        public Album()
        {                  
            Images = new List<string>();
            UploadProgress = 0;
        }

        public int? ImageCount()
        { return Images.Count; }

    }
}
