﻿using iTEMS.Models;
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

        public DbSet<Project> Project { get; set; } = default!;
        public DbSet<InAppNotification> InAppNotification { get; set; } = default!;

        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentFile> CommentFiles { get; set; } // Add this DbSet

        public DbSet<TimelineFile> TimelineFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<CommentFile>()
                .HasOne(cf => cf.Comment)
                .WithMany(c => c.CommentFiles)
                .HasForeignKey(cf => cf.CommentId);

            // Other configurations...
        }
    }
}
