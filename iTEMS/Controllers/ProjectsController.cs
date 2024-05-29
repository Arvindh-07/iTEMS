﻿using System;
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
        public async Task<IActionResult> Details(int? id)
        {
            await SetNotificationsInViewBag();
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Timelines) // Include the timeline entries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

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
                            Description = $"{currentUser.UserName} posted an update: {project.Update}",
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
                            Description = $"{currentUser.UserName} changed the status to: {project.Status}",
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
                return BadRequest();
            }

            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User); // Get the current logged-in user

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

            await _context.SaveChangesAsync();

            var uploadedFilePaths = await UploadFiles(files);
            if (project.Attachments == null)
            {
                project.Attachments = new List<string>();
            }
            project.Attachments.AddRange(uploadedFilePaths);

            await _context.SaveChangesAsync();
            TempData["UploadSuccess"] = "Files uploaded successfully!";

            return RedirectToAction("Details", new { id = id });
        }



    }
}