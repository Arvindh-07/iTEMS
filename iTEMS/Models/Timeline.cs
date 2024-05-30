using System;

namespace iTEMS.Models
{
    public class Timeline
    {
        public int Id { get; set; }
        public string? Type { get; set; } // Type of activity (e.g., "File Upload", "Status Change", etc.)
        public string? Description { get; set; } // Description of the activity
        public DateTime Timestamp { get; set; } // Timestamp of when the activity occurred
        public int ProjectId { get; set; } // Foreign key to associate the activity with a project

        public string? FileName { get; set; } // Add file name property
        public string? UserInvolved { get; set; } // Add user involved property

        // Navigation property to relate Timeline to Project
        public Project? Project { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();

        // You can add more properties as needed, such as UserId to track who performed the activity
    }

    public class Comment
    {
        public int Id { get; set; }
        public string? User { get; set; }
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime? EditedTimestamp { get; set; } // Nullable to indicate if the comment was edited
        public bool IsEdited { get; set; }

        public string? FilePath { get; set; } // To store the file path
        public string? FileName { get; set; } // To store the original file name
        public string? FilePathComment { get; set; } // To store the file path
        public string? FileNameComment { get; set; } // To store the original file name

        public List<CommentFile> CommentFiles { get; set; } = new List<CommentFile>();
    }

    public class CommentFile
    {
        public int Id { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public int CommentId { get; set; }
        public Comment Comment { get; set; }
    }



}
