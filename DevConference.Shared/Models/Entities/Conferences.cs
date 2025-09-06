using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevConference.Shared.Models.Entities
{
    public class Conferences
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }
        public string? ImageUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public bool IsActive { get; set; }
        public int ConferenceStatus { get; set; } //1 - Upcoming, 2 - Ongoing, 3 - Completed
        // public ICollection<Sessions>? Sessions { get; set; }
    }
}
