using iTEMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using iTEMS.Data;

namespace iTEMS.Controllers
{
    //[Authorize]
    public class HomeController : BaseController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager, ApplicationDbContext context)
            : base(context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        //[Authorize]
        public async Task<IActionResult> Index()
        {
            //// Retrieve the current user
            //var currentUser = await _userManager.GetUserAsync(User);

            //// Retrieve the employee based on the current user's email
            //var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == currentUser.UserName);

            //// Check if an employee record with a matching email was found
            //if (employee != null)
            //{
            //    // If a matching employee record is found, display the first name
            //    ViewBag.DisplayName = employee.FirstName;
            //}
            //else
            //{
            //    // If no matching employee record is found, display the email
            //    ViewBag.DisplayName = "Test";
            //}

            //// Call the SetNotificationsInViewBag method from the base controller to set notifications
            //await SetNotificationsInViewBag();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
