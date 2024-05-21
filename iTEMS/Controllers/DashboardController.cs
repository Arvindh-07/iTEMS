using iTEMS.Data;
using iTEMS.Models;
using iTEMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var activeTasks = await _context.TaskTrackers
        .Include(t => t.Project) // Include the project data
        .Where(t => t.Status == "Planning" || t.Status == "Pending" || t.Status == "Delayed" || t.Status == "Blocked")
        .ToListAsync();

        var viewModel = new DashboardViewModel
        {
            

            TotalProjects = _context.Project.Count(),
            PlanningProjects = _context.Project.Count(p => p.Status == ProjectStatus.Planning),
            PendingProjects = _context.Project.Count(p => p.Status == ProjectStatus.Pending),
            DelayedProjects = _context.Project.Count(p => p.Status == ProjectStatus.Delayed),
            BlockedProjects = _context.Project.Count(p => p.Status == ProjectStatus.Blocked),
            CompletedProjects = _context.Project.Count(p => p.Status == ProjectStatus.Completed),
            ActiveProjectsList = _context.Project.Where(p => p.Status == ProjectStatus.Planning || p.Status == ProjectStatus.Pending || p.Status == ProjectStatus.Delayed || p.Status == ProjectStatus.Blocked || p.Status == ProjectStatus.Completed).ToList(),
            TeamMembers = _context.Employees.ToList(),
            TotalTasks = _context.TaskTrackers.Count(),
            PlanningTasks = _context.TaskTrackers.Count(t => t.Status == "Planning"),
            PendingTasks = _context.TaskTrackers.Count(t => t.Status == "Pending"),
            DelayedTasks = _context.TaskTrackers.Count(t => t.Status == "Delayed"),
            BlockedTasks = _context.TaskTrackers.Count(t => t.Status == "Blocked"),
            CompletedTasks = _context.TaskTrackers.Count(t => t.Status == "Completed"),
            ActiveTasksList = _context.TaskTrackers.Where(t => t.Status == "Planning" || t.Status == "Pending" || t.Status == "Delayed" || t.Status == "Blocked").ToList(),

            TeamMembersProjects = new Dictionary<Employee, List<Project>>(),
            TeamMembersTasks = new Dictionary<Employee, List<TaskTracker>>(),
            TaskAssignments = new Dictionary<int, string>()
        };

        var employees = await _context.Employees.ToListAsync();
        

        foreach (var task in viewModel.ActiveTasksList)
        {
            var employee = employees.FirstOrDefault(e => e.Id == task.AssignedTo);
            if (employee != null)
            {
                viewModel.TaskAssignments[task.Id] = employee.FullName;
            }
        }

        foreach (var member in viewModel.TeamMembers)
        {
            var projects = await _context.Project
                .Where(p => p.Tasks.Any(t => t.AssignedTo == member.Id) &&
                            p.Status != ProjectStatus.Completed)
                .ToListAsync();

            var tasks = await _context.TaskTrackers
                .Where(t => t.AssignedTo == member.Id &&
                            t.Status != TaskTracker.TaskTrackerStatus.Completed.ToString())
                .ToListAsync();

            viewModel.TeamMembersProjects.Add(member, projects);
            viewModel.TeamMembersTasks.Add(member, tasks);
        }


        var projectTaskDetailsList = new List<ProjectTaskDetails>();

        foreach (var project in viewModel.ActiveProjectsList)
        {
            var projectTaskDetails = new ProjectTaskDetails
            {
                Project = project,
                Tasks = await _context.TaskTrackers
                            .Where(t => t.ProjectId == project.Id)
                            .ToListAsync()
            };

            projectTaskDetailsList.Add(projectTaskDetails);
        }

        viewModel.ProjectTaskDetailsList = projectTaskDetailsList;



        return View(viewModel);
    }
}
