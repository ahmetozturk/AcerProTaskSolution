namespace AcerProTask.Models
{
    public class HealthCheckLogViewModel
    {
        public List<HealthCheckLog> HealthCheckLogs { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public DateTime FilterDate { get; set; }
        public string FilterAppName { get; set; }

  
    }

}
