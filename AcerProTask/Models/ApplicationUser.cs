using Microsoft.AspNetCore.Identity;

namespace AcerProTask.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
        public ICollection<TargetApp> TargetApps { get; set; }
    }
}
