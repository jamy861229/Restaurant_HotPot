
using Microsoft.EntityFrameworkCore;

namespace Restaurant.Models
{
    [Keyless]
    public class HomepageView
    {
        public int Carousel_CarouselId { get; set; }
        public string? Carousel_ImageUrl { get; set; }
        public string? Carousel_Title { get; set; }
        public string? Carousel_Description { get; set; }
        public string? Carousel_LinkUrl { get; set; }
        public int Carousel_DisplayOrder { get; set; }
        public bool Carousel_IsActive { get; set; }

        public DateTime? Carousel_StartTime { get; set; }
        public DateTime? Carousel_EndTime { get; set; }
        public DateTime? Carousel_CreatedAt { get; set; }
    }
}
