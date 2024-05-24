
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using iTEMS.Data;
using iTEMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using static iTEMS.Models.TaskTracker;

namespace iTEMS.Controllers
{
    [Authorize]
    public class TaskTrackersController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TaskTrackersController> _logger; // Injected logger
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public TaskTrackersController(ApplicationDbContext context, ILogger<TaskTrackersController> logger, UserManager<IdentityUser> userManager, IWebHostEnvironment environment) : base(context)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _environment = environment;
        }

        private async Task<List<InAppNotification>> GetNotificationsForCurrentUser(string userName)
        {

            return await _context.InAppNotifications
                .Where(n => n.UserName == userName)
                .OrderByDescending(n => n.Timestamp)
                .ToListAsync();
        }

        // GET: TaskTrackers
        public async Task<IActionResult> Index()
        {

            await SetNotificationsInViewBag();
            var applicationDbContext = _context.TaskTrackers
                .Include(t => t.Project)
                .Include(t => t.Employee); // Include the Employee navigation property

            return View(await applicationDbContext.ToListAsync());
        }


        // GET: TaskTrackers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            await SetNotificationsInViewBag();
            if (id == null)
            {
                return NotFound();
            }

            var taskTracker = await _context.TaskTrackers
                .Include(t => t.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskTracker == null)
            {
                return NotFound();
            }

            return View(taskTracker);
        }

        // GET: TaskTrackers/Create
        public async Task<IActionResult> Create()
        {
            await SetNotificationsInViewBag();
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name");
            ViewData["AssignedTo"] = new SelectList(_context.Employees, "Id", "UserName");
            ViewData["StatusList"] = new SelectList(Enum.GetValues(typeof(TaskTrackerStatus)));
            ViewData["PriorityList"] = new SelectList(Enum.GetValues(typeof(TaskPriority))); // Add this line

            return View();
        }

        // POST: TaskTrackers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,AssignedTo,Status,Priority,DueDate,StartDate,EstimatedTime,ActualTime,Tags,Attachments,Comments,ProjectId,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn")] TaskTracker taskTracker, List<IFormFile> files)
        {
            await SetNotificationsInViewBag();
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var assignedUser = await _context.Employees.FindAsync(taskTracker.AssignedTo);
                var uploadsDirectory = Path.Combine(_environment.WebRootPath, "uploads"); // Assuming _environment is an injected IWebHostEnvironment
                if (!Directory.Exists(uploadsDirectory))
                {
                    Directory.CreateDirectory(uploadsDirectory);
                }
                taskTracker.CreatedBy = currentUser.UserName;
                taskTracker.CreatedOn = DateTime.Now; // Set the creation date here
                taskTracker.ModifiedBy = currentUser.UserName;
                taskTracker.ModifiedOn = DateTime.Now;

                // Process file uploads
                if (files != null && files.Count > 0)
                {
                    taskTracker.FilePaths = new List<string>();

                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Path.GetFileName(file.FileName); // Corrected line
                            var filePath = Path.Combine(uploadsDirectory, fileName);

                            // Save the file to the server
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            // Add the file path to the TaskTracker's FilePath list
                            taskTracker.FilePaths.Add(filePath);
                        }
                    }

                }

                _context.Add(taskTracker);
                await _context.SaveChangesAsync();

                if (assignedUser != null)
                {
                    // Check if a notification has already been sent to the assigned user for this task
                    var existingNotification = await _context.Notifications
                        .FirstOrDefaultAsync(n => n.UserName == assignedUser.UserName && n.Message == $"You have been assigned a new task: {taskTracker.Title}");

                    if (existingNotification == null)
                    {
                        var notification = new Notification
                        {
                            Message = $"You have been assigned a new task: {taskTracker.Title}",
                            Timestamp = DateTime.Now,
                            UserName = assignedUser.UserName, // Assuming UserName is the property containing the username
                            Employee = assignedUser
                        };

                        _context.Add(notification);
                        await _context.SaveChangesAsync();

                        // Send in-application notification
                        await SendInAppNotification(assignedUser.UserName, notification.Message);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while creating a TaskTracker.");

                // Optionally, you can add the exception message to the ModelState
                ModelState.AddModelError("", "An error occurred while saving the task.");
            }

            // If ModelState is invalid or an exception occurred, return to the Create view with error messages
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name", taskTracker.ProjectId);
            ViewData["AssignedTo"] = new SelectList(_context.Employees, "Id", "UserName", taskTracker.Employee);
            ViewData["StatusList"] = new SelectList(Enum.GetValues(typeof(TaskTrackerStatus)), taskTracker.Status);
            ViewData["PriorityList"] = new SelectList(Enum.GetValues(typeof(TaskPriority)), taskTracker.Priority); // Add this line
            return View(taskTracker);
        }



        // GET: TaskTrackers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            await SetNotificationsInViewBag();
            if (id == null)
            {
                return NotFound();
            }

            var taskTracker = await _context.TaskTrackers.FindAsync(id);
            if (taskTracker == null)
            {
                return NotFound();
            }

            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name", taskTracker.ProjectId);
            ViewData["AssignedTo"] = new SelectList(_context.Employees, "Id", "UserName", taskTracker.Employee);
            ViewData["StatusList"] = new SelectList(Enum.GetValues(typeof(TaskTrackerStatus)), taskTracker.Status);
            ViewData["PriorityList"] = new SelectList(Enum.GetValues(typeof(TaskPriority)), taskTracker.Priority); // Add this line
            return View(taskTracker);
        }




        // POST: TaskTrackers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,AssignedTo,Status,Priority,DueDate,StartDate,EstimatedTime,ActualTime,Tags,Attachments,Comments,ProjectId,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn")] TaskTracker taskTracker, List<IFormFile> files)
        {
            await SetNotificationsInViewBag();
            if (id != taskTracker.Id)
            {
                return NotFound();
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                // Assign the current user's username to the ModifiedBy property
                taskTracker.ModifiedBy = currentUser.UserName;

                // Set the ModifiedOn property to the current date and time
                taskTracker.ModifiedOn = DateTime.Now;

                if (taskTracker.Status == TaskTrackerStatus.Completed.ToString())
                {
                    taskTracker.UpdateActualTime();
                }

                // Process file uploads
                var uploadsDirectory = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsDirectory))
                {
                    Directory.CreateDirectory(uploadsDirectory);
                }

                if (files != null && files.Count > 0)
                {
                    taskTracker.FilePaths = new List<string>();

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

                            // Add the file path to the TaskTracker's FilePath list
                            taskTracker.FilePaths.Add(filePath);
                        }
                    }
                }

                _context.Update(taskTracker);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskTrackerExists(taskTracker.Id))
                {
                    ModelState.AddModelError("", "An error occurred while saving the task. Task does not exist.");
                    return NotFound();
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred while saving the task.");
                    throw;
                }
            }

            // If ModelState is not valid, return to the Edit view with error messages
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name", taskTracker.ProjectId);
            ViewData["AssignedTo"] = new SelectList(_context.Employees, "Id", "UserName", taskTracker.Employee);
            ViewData["StatusList"] = new SelectList(Enum.GetValues(typeof(TaskTrackerStatus)), taskTracker.Status);
            ViewData["PriorityList"] = new SelectList(Enum.GetValues(typeof(TaskPriority)), taskTracker.Priority);

            // Find the first error in ModelState and add an error message indicating the unfilled field
            var firstError = ModelState.Values.FirstOrDefault(v => v.Errors.Any());
            if (firstError != null)
            {
                var unfilledField = firstError.Errors.FirstOrDefault()?.ErrorMessage;
                ModelState.AddModelError("", $"{unfilledField}");
            }
            else
            {
                ModelState.AddModelError("", "Please fill out all required fields.");
            }

            return View(taskTracker);
        }




        // GET: TaskTrackers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            await SetNotificationsInViewBag();
            if (id == null)
            {
                return NotFound();
            }

            var taskTracker = await _context.TaskTrackers
                .Include(t => t.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskTracker == null)
            {
                return NotFound();
            }

            return View(taskTracker);
        }

        // POST: TaskTrackers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await SetNotificationsInViewBag();
            var taskTracker = await _context.TaskTrackers.FindAsync(id);
            if (taskTracker != null)
            {
                _context.TaskTrackers.Remove(taskTracker);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task SendInAppNotification(string userName, string message)
        {
            var inAppNotification = new InAppNotification
            {
                UserName = userName,
                Message = message,
                Timestamp = DateTime.Now
            };

            _context.Add(inAppNotification);
            await _context.SaveChangesAsync();
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Notifications()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var notifications = await GetNotificationsForCurrentUser(currentUser.UserName);
            return PartialView("_NotificationsPartial", notifications);
        }

        private bool TaskTrackerExists(int id)
        {
            return _context.TaskTrackers.Any(e => e.Id == id);
        }

        public async Task<IActionResult> DownloadFile(int id)
        {
            var taskTracker = await _context.TaskTrackers.FindAsync(id);

            if (taskTracker == null)
            {
                return NotFound();
            }

            // Assuming taskTracker.FilePaths contains the file path(s)
            var filePath = taskTracker.FilePaths.FirstOrDefault(); // Assuming there's only one file path per taskTracker

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
            var fileName = Path.GetFileName(filePath);

            return File(memoryStream, contentType, fileName);
        }
    }
}
