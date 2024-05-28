using System;

namespace iTEMS.Models
{
    public class Timeline
    {
        public int Id { get; set; }
        public string Type { get; set; } // Type of activity (e.g., "File Upload", "Status Change", etc.)
        public string Description { get; set; } // Description of the activity
        public DateTime Timestamp { get; set; } // Timestamp of when the activity occurred
        public int ProjectId { get; set; } // Foreign key to associate the activity with a project

        public string FileName { get; set; } // Add file name property
        public string UserInvolved { get; set; } // Add user involved property

        // Navigation property to relate Timeline to Project
        public Project Project { get; set; }

        // You can add more properties as needed, such as UserId to track who performed the activity
    }
}
