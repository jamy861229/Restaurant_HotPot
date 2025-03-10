
using Microsoft.EntityFrameworkCore;

namespace Restaurant.Models
{
    [Keyless]
    public class HomepageView
    {
        public int CarouselId { get; set; }
        public string? CarouselImageUrl { get; set; }
        public string? CarouselTitle { get; set; }

        public int CarouselDisplayOrder { get; set; }
        public bool CarouselIsActive { get; set; }

        public DateTime? CarouselStartTime { get; set; }
        public DateTime? CarouselEndTime { get; set; }
        public DateTime? CarouselCreatedAt { get; set; }
    }
}
