using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iTEMS.Models
{
    public class TaskTracker : UserActivity
    {

        public int Id { get; set; }

        
        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        
        [Display(Name = "Assignee")]
        public string? Assignee { get; set; }

        
        [Display(Name = "Status")]
        public string? Status { get; set; }

        
        [Display(Name = "Priority")]
        public TaskPriority Priority { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        
        [Display(Name = "Estimated Time")]
        public string? EstimatedTime { get; set; }

        [Display(Name = "Actual Time")]
        public string? ActualTime { get; set; }

        [Display(Name = "Tags")]
        public string? Tags { get; set; }

        [Display(Name = "Attachments")]
        public string? Attachments { get; set; }

        [Display(Name = "File Paths")]
        public List<string>? FilePaths { get; set; }

        [Display(Name = "Comments")]
        public string? Comments { get; set; }

        [Display(Name = "Project ID")]
        public int ProjectId { get; set; } // Foreign key property

        [ForeignKey("ProjectId")]
        public Project Project { get; set; } // Navigation property

        [Display(Name = "Assigned To")]
        public int AssignedTo { get; set; }

        [ForeignKey("AssignedTo")]
        public Employee Employee { get; set; }


        // Consider adding navigation properties for dependencies and related tasks if using Entity Framework

        // Example navigation properties:
        // public ICollection<TaskItem> Dependencies { get; set; }
        // public ICollection<TaskItem> RelatedTasks { get; set; }

        public enum TaskTrackerStatus
        {
            Planning,
            Pending,
            Completed,
            Delayed,
            Blocked

        }

        public enum TaskPriority
        {
            High = 1,
            Medium = 2,
            Low = 3
        }


        public void UpdateActualTime()
        {
            if (Status == TaskTrackerStatus.Completed.ToString())
            {
                var timeSpan = DateTime.Now - StartDate;
                ActualTime = $"{timeSpan.Days} days, {timeSpan.Hours} hours, {timeSpan.Minutes} minutes";
            }
        }



    }
}
