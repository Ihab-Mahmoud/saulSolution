using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Saul.DataAccess.Data;
using Saul.Models;
using Saul.Utility;

namespace Saul.DbInitializer
{
    public class DbInitializer : IDbInitializer

    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
        }

        public void Initialize()
        {
            try
            {

                    _db.Database.Migrate();
            }
            catch (Exception)
            {

                throw;
            }

            if (!_roleManager.RoleExistsAsync(SD.Role_Cust).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Cust)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Comp)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();


                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@dotnetmastery.com",
                    Email = "admin@dotnetmastery.com",
                    Name = "Ihab Ayman",
                    PhoneNumber = "1234567890",
                    PostalCode = "1234567890",
                    Region = "Il",
                    City = "LA",
                    StreetAddress = "6th avenue"

                }, "Admin123@").GetAwaiter().GetResult();


                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u=>u.Email== "admin@dotnetmastery.com");

                _userManager.AddToRoleAsync(user,SD.Role_Admin).GetAwaiter().GetResult();
            }

            return;
        }
    }
}
