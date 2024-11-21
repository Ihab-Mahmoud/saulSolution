using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Saul.DataAccess.Data;
using Saul.DataAccess.Repository;
using Saul.DataAccess.Repository.IRepository;
using Saul.Models;
using Saul.Models.ViewModels;
using Saul.Utility;
using Stripe.Radar;
using System.Collections;
using System.Data;

namespace Saul.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public UserController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, ApplicationDbContext db,IUnitOfWork unitOfWork)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
            _unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {
            return View();
        }

        public  async Task<IActionResult> UserRole(string userId)

        {
            ApplicationUser user = _unitOfWork.applicationUser.Get(u=>u.Id == userId,includeProperties:"Company");
            var roles = await _userManager.GetRolesAsync(user);
            string currentRole = roles.FirstOrDefault(); // Gets the first role, or null if there are no roles.


            
            user.Role = currentRole;

            UserRoleVM userRoleVM = new UserRoleVM()
            {
                ApplicationUser = user,
                CompanyList = _unitOfWork.company.GetAll().Select(u => new SelectListItem
                  {
                      Text = u.Name,
                      Value = u.Id.ToString()
                  }),
                RoleList = _roleManager.Roles.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Name
                }),
            };

            return View(userRoleVM);
        }


        [HttpPost]
        public async Task<IActionResult> UserRole(UserRoleVM userRoleVM)
        {
            // Retrieve the user from the database
            ApplicationUser user = _unitOfWork.applicationUser.Get(u => u.Id == userRoleVM.ApplicationUser.Id);

            Company company = _unitOfWork.company.Get(u => u.Id == userRoleVM.ApplicationUser.CompanyId);
            // Check if the user exists
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Update user properties
            user.Name = userRoleVM.ApplicationUser.Name;
            if (userRoleVM.ApplicationUser.CompanyId!=null && userRoleVM.ApplicationUser.Role==SD.Role_Comp)
            {
                user.CompanyId = userRoleVM.ApplicationUser.CompanyId;
                user.Company = company;
            }
            else
            {
                user.CompanyId = null;
            }

            // Save changes to the database
            _unitOfWork.applicationUser.update(user);
            _unitOfWork.save();

            // Get current roles of the user
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove all current roles
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return BadRequest("Failed to remove existing roles.");
            }

            // Add the user to the new role
            var addResult = await _userManager.AddToRoleAsync(user, userRoleVM.ApplicationUser.Role);
            if (!addResult.Succeeded)
            {
                return BadRequest("Failed to assign new role.");
            }

            return RedirectToAction("Index");
        }



        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<ApplicationUser> users = _unitOfWork.applicationUser.GetAll( includeProperties: "Company").ToList();

            


            foreach (var item in users)
            {

                var roles = await _userManager.GetRolesAsync(item);
                string currentRole = roles.FirstOrDefault(); // Gets the first role, or null if there are no roles.



                item.Role = currentRole;
                if (item.Company == null)
                {
                    item.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }
            return Json(new { data = users });
        }

        [HttpPost]
        public IActionResult LockUnlock(string id)
        {

            ApplicationUser user = _unitOfWork.applicationUser.Get(u=>u.Id==id);

            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                user.LockoutEnd = null;
                _unitOfWork.applicationUser.update(user);
                _unitOfWork.save();

                return Json(new { success = "true", message = "User has been unlocked successfully" });

            }
            else
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(5);
                _unitOfWork.applicationUser.update(user);
                _unitOfWork.save();

                return Json(new { success = "true", message = "User has been locked successfully" });

            }

        }


        #endregion
    }
}
