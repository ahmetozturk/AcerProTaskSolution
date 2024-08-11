using AcerProTask.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AcerProTask.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<TargetApp> TargetApps { get; set; }
        public DbSet<NotificationSettings> NotificationSettings { get; set; }
        public DbSet<HealthCheckLog> HealthCheckLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<TargetApp>()
                .HasOne(app => app.User)
                .WithMany()
                .HasForeignKey(app => app.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HealthCheckLog>()
        .HasOne(h => h.TargetApp)
        .WithMany(t => t.HealthCheckLogs)
        .HasForeignKey(h => h.TargetAppId);



        }
    }



}
