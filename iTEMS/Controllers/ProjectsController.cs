using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using iTEMS.Data;
using iTEMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using iTEMS.Data.Migrations;

namespace iTEMS.Controllers
{
    [Authorize]
    public class ProjectsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ProjectsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment environment) : base(context)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            await SetNotificationsInViewBag();
            return View(await _context.Project.ToListAsync());
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var project = await _context.Project
        .Include(p => p.Timelines)
            .ThenInclude(t => t.Comments)
                .ThenInclude(c => c.CommentFiles)
        .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            ViewData["TotalComments"] = project.Timelines.SelectMany(t => t.Comments).Count();
            ViewData["CommentBatchSize"] = 5;

            return View(project);
        }

        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {

            await SetNotificationsInViewBag();
            var currentUser = await _userManager.GetUserAsync(User);
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == currentUser.UserName);
            ViewData["StatusList"] = new SelectList(Enum.GetValues(typeof(ProjectStatus)));
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,StartDate,EndDate,Status,PIC,Budget,Update,Blocker,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,EstimatedDuration,TotalSpent,ClientName")] Project project, List<IFormFile> files)
        {
            await SetNotificationsInViewBag();
            if (ModelState.IsValid)
            {
                project.Status = ProjectStatus.Planning;
                project.Timelines = new List<iTEMS.Models.Timeline>(); // Initialize the Timelines list

                try
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    var uploadsDirectory = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsDirectory))
                    {
                        Directory.CreateDirectory(uploadsDirectory);
                    }

                    project.CreatedBy = currentUser.UserName;
                    project.CreatedOn = DateTime.Now;
                    project.ModifiedBy = currentUser.UserName;
                    project.ModifiedOn = DateTime.Now;

                    if (files != null && files.Count > 0)
                    {
                        project.Attachments = new List<string>();

                        foreach (var file in files)
                        {
                            if (file.Length > 0)
                            {
                                var fileName = Path.GetFileName(file.FileName);
                                var filePath = Path.Combine(uploadsDirectory, fileName);

                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(fileStream);
                                }

                                project.Attachments.Add(filePath);
                            }
                        }
                    }

                    _context.Add(project);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving the project.");
                    return View(project);
                }
            }

            ViewData["StatusList"] = new SelectList(Enum.GetValues(typeof(ProjectStatus)), project.Status);
            return View(project);
        }


        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            await SetNotificationsInViewBag();
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Timelines) // Include timelines
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            ViewData["StatusList"] = new SelectList(Enum.GetValues(typeof(ProjectStatus)), project.Status);
            return View(project);
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,StartDate,EndDate,Status,PIC,Budget,Update,Blocker,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,EstimatedDuration,TotalSpent,ClientName,Timelines")] Project project, List<IFormFile> files)
        {
            await SetNotificationsInViewBag();
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _userManager.GetUserAsync(User);

                    // Load existing project from the database, including timelines
                    var existingProject = await _context.Project
                        .Include(p => p.Timelines)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (existingProject == null)
                    {
                        return NotFound();
                    }

                    if (existingProject.Update != project.Update)
                    {
                        // Create a new timeline entry
                        var timelineEntry = new iTEMS.Models.Timeline
                        {
                            ProjectId = project.Id,
                            Type = "Update Posted",
                            Description = $"Project Update: {project.Update}",
                            UserInvolved = currentUser.UserName,
                            Timestamp = DateTime.Now
                        };

                        // Add the new timeline entry to the database
                        _context.Timelines.Add(timelineEntry);
                    }

                    if (existingProject.Blocker != project.Blocker)
                    {
                        // Create a new timeline entry
                        var timelineEntry = new iTEMS.Models.Timeline
                        {
                            ProjectId = project.Id,
                            Type = "Update Posted",
                            Description = $"Project Blocker Update: {project.Blocker}",
                            UserInvolved = currentUser.UserName,
                            Timestamp = DateTime.Now
                        };

                        // Add the new timeline entry to the database
                        _context.Timelines.Add(timelineEntry);
                    }

                    if (existingProject.Status != project.Status)
                    {
                        var statusChangeEntry = new iTEMS.Models.Timeline
                        {
                            ProjectId = project.Id,
                            Type = "Status Change",
                            Description = $"Project Status Update: {project.Status}",
                            UserInvolved = currentUser.UserName,
                            Timestamp = DateTime.Now
                        };

                        // Add the status change entry to the database
                        _context.Timelines.Add(statusChangeEntry);
                    }


                    // Preserve existing attachments
                    project.Attachments = existingProject.Attachments ?? new List<string>();

                    // Preserve existing timelines
                    project.Timelines = existingProject.Timelines ?? new List<iTEMS.Models.Timeline>();

                    // Handle file uploads
                    var uploadedFilePaths = await UploadFiles(files);
                    if (uploadedFilePaths.Count > 0)
                    {
                        project.Attachments.AddRange(uploadedFilePaths);
                    }

                    // Assign the current user's username to the ModifiedBy property
                    project.ModifiedBy = currentUser.UserName;

                    // Set the ModifiedOn property to the current date and time
                    project.ModifiedOn = DateTime.Now;

                    // Update the project
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Log the validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                }
            }

            ViewData["StatusList"] = new SelectList(Enum.GetValues(typeof(ProjectStatus)), project.Status);
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostUpdate(int id, string update)
        {
            if (string.IsNullOrWhiteSpace(update))
            {
                return RedirectToAction("Details", new { id });
            }

            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var timelineEntry = new iTEMS.Models.Timeline
            {
                ProjectId = project.Id,
                Type = "Update Posted",
                Description = update,
                UserInvolved = currentUser.UserName,
                Timestamp = DateTime.Now
            };

            _context.Timelines.Add(timelineEntry);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id });
        }






        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            await SetNotificationsInViewBag();
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await SetNotificationsInViewBag();
            var project = await _context.Project.FindAsync(id);
            if (project != null)
            {
                _context.Project.Remove(project);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Project.Any(e => e.Id == id);
        }

        public async Task<IActionResult> DownloadFile(int id, string fileName)
        {
            var project = await _context.Project.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            var filePath = project.Attachments?.FirstOrDefault(f => Path.GetFileName(f) == fileName);

            if (filePath == null || !System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memoryStream);
            }

            memoryStream.Position = 0;
            var contentType = "application/octet-stream"; // Set the content type based on your file type
            var downloadFileName = Path.GetFileName(filePath);

            return File(memoryStream, contentType, downloadFileName);
        }



        private async Task<List<string>> UploadFiles(List<IFormFile> files)
        {
            var uploadedFilePaths = new List<string>();
            var uploadsDirectory = Path.Combine(_environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(uploadsDirectory, fileName);

                        // Save the file to the server
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        // Add the file path to the list of uploaded file paths
                        uploadedFilePaths.Add(filePath);
                    }
                }
            }

            return uploadedFilePaths;
        }

        // POST: Projects/UploadFiles/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFiles(int id, List<IFormFile> files)
        {
            if (id <= 0 || files == null || files.Count == 0)
            {
                return BadRequest("Invalid project ID or no files selected.");
            }

            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                return NotFound("Project not found.");
            }

            var currentUser = await _userManager.GetUserAsync(User); // Get the current logged-in user

            try
            {
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file.FileName); // Get the file name
                    var timelineEntry = new iTEMS.Models.Timeline
                    {
                        ProjectId = project.Id,
                        Type = "File Uploaded",
                        Description = $"{currentUser.UserName} uploaded file: {fileName}",
                        FileName = fileName, // Set the file name here
                        UserInvolved = currentUser.UserName, // Set the user involved here
                        Timestamp = DateTime.Now
                    };

                    // Save the timeline entry to the database
                    _context.Timelines.Add(timelineEntry);
                }

                await _context.SaveChangesAsync(); // Save all changes to the database

                var uploadedFilePaths = await UploadFilesToServer(files); // Assuming you have a method for handling file uploads
                if (project.Attachments == null)
                {
                    project.Attachments = new List<string>();
                }
                project.Attachments.AddRange(uploadedFilePaths);

                await _context.SaveChangesAsync();
                TempData["UploadSuccess"] = "Files uploaded successfully!";
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while uploading files.");
            }

            return RedirectToAction("Details", new { id = id });
        }

        private async Task<List<string>> UploadFilesToServer(List<IFormFile> files)
        {
            var uploadedFilePaths = new List<string>();
            var uploadsDirectory = Path.Combine(_environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(uploadsDirectory, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    uploadedFilePaths.Add(filePath);
                }
            }

            return uploadedFilePaths;
        }





        // GET: Projects/DeleteTimelineEntry/5
        public async Task<IActionResult> DeleteTimelineEntry(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var timelineEntry = await _context.Timelines
                .FirstOrDefaultAsync(m => m.Id == id);
            if (timelineEntry == null)
            {
                return NotFound();
            }

            // You can skip the confirmation view and directly ask for confirmation using JavaScript in the UI.
            return RedirectToAction(nameof(Index)); // Remove this line if you're handling the confirmation directly.
        }

        // POST: Projects/DeleteTimelineEntry/5
        // POST: Projects/DeleteTimelineEntry/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTimelineEntryConfirmed(int id)
        {
            var timelineEntry = await _context.Timelines.FindAsync(id);
            if (timelineEntry != null)
            {
                var projectId = timelineEntry.ProjectId; // Get the associated ProjectId
                _context.Timelines.Remove(timelineEntry);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Timeline entry deleted successfully.";

                // Redirect to the Project Details page
                return RedirectToAction("Details", "Projects", new { id = projectId });
            }
            return RedirectToAction(nameof(Index)); // If not found, redirect to Index or handle appropriately
        }

        [HttpPost]
        public async Task<IActionResult> PostComment(int id, string comment, List<IFormFile> files)
        {
            var timelineEntry = await _context.Timelines.Include(t => t.Comments).ThenInclude(c => c.CommentFiles).FirstOrDefaultAsync(t => t.Id == id);
            var currentUser = await _userManager.GetUserAsync(User);

            if (timelineEntry != null && currentUser != null)
            {
                var newComment = new Comment
                {
                    User = currentUser.UserName,
                    Content = comment,
                    Timestamp = DateTime.Now
                };

                if (files != null && files.Count > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/comments");
                    Directory.CreateDirectory(uploadsFolder); // Ensure the folder exists

                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var filePath = Path.Combine(uploadsFolder, fileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            var commentFile = new CommentFile
                            {
                                FilePath = $"uploads/comments/{fileName}",
                                FileName = fileName
                            };
                            newComment.CommentFiles.Add(commentFile);
                        }
                    }
                }

                timelineEntry.Comments.Add(newComment);
                _context.Update(timelineEntry);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", new { id = timelineEntry.ProjectId });
            }

            return NotFound();
        }




        [HttpPost]
        public async Task<IActionResult> EditComment(int commentId, string comment)
        {
            var timelineEntry = await _context.Timelines
                                              .Include(t => t.Comments)
                                              .FirstOrDefaultAsync(t => t.Comments.Any(c => c.Id == commentId));

            if (timelineEntry != null)
            {
                var existingComment = timelineEntry.Comments.FirstOrDefault(c => c.Id == commentId);
                if (existingComment != null)
                {
                    existingComment.Content = comment;
                    existingComment.EditedTimestamp = DateTime.Now;
                    existingComment.IsEdited = true;
                    _context.Update(timelineEntry);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("Details", new { id = timelineEntry.ProjectId });
            }
            return NotFound();
        }





        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var timelineEntry = await _context.Timelines
                                              .Include(t => t.Comments)
                                              .FirstOrDefaultAsync(t => t.Comments.Any(c => c.Id == id));

            if (timelineEntry != null)
            {
                var projectId = timelineEntry.ProjectId;
                var comment = timelineEntry.Comments.FirstOrDefault(c => c.Id == id);

                if (comment != null)
                {
                    timelineEntry.Comments.Remove(comment);
                    _context.Remove(comment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = projectId });
                }
            }

            return NotFound();
        }







    }
}