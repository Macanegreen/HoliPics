

namespace HoliPics.Models
{
    public class Album
    {
        public int Id { get; set; }
        public string? CreatorId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreationTime { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public List<string> Images { get; set; }
        
        public Album()
        {                  
            Images = new List<string>();
        }
    }
}
