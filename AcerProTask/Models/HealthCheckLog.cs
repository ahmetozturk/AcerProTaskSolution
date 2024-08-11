namespace AcerProTask.Models
{
    public class HealthCheckLog
    {
        public int Id { get; set; }
        public int TargetAppId { get; set; }
        public TargetApp TargetApp { get; set; }
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
