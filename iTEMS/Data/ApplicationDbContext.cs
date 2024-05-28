using iTEMS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace iTEMS.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<TaskTracker> TaskTrackers { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Timeline> Timelines { get; set; } // Add this DbSet for the Timeline model
        public DbSet<InAppNotification> InAppNotifications { get; set; }

        public DbSet<iTEMS.Models.Project> Project { get; set; } = default!;
        public DbSet<iTEMS.Models.InAppNotification> InAppNotification { get; set; } = default!;


    }
}
