using System.ComponentModel.DataAnnotations;

namespace AcerProTask.Models
{
	public class TargetApp
	{
		public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "URL is required")]
        [Url(ErrorMessage = "Invalid URL format")]
        public string Url { get; set; }
        [Required(ErrorMessage = "Monitoring Interval is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Monitoring Interval must be greater than 0")]
        public int MonitoringInterval { get; set; } 


        public string UserId { get; set; }
		public ApplicationUser User { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime LastUpdated { get; set; }

        public ICollection<HealthCheckLog> HealthCheckLogs { get; set; }
    }

}
