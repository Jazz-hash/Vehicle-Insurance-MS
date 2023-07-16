using System;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using CFMS.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

[assembly: OwinStartupAttribute(typeof(CFMS.Startup))]
namespace CFMS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            AddUserAndRoles();
        }
      
        private void AddUserAndRoles()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var roleManger = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            if (!roleManger.RoleExists("Admin"))
            {
                var role = new IdentityRole();
                role.Name = "Admin";
                roleManger.Create(role);
                var user = new ApplicationUser()
                {
                    UserName = "JazzAdmin",
                    Email = "jazzelmehmood4@gmail.com",
                    EmailConfirmed = true,
                    DateOfJoining = DateTime.Now,
                    Name = "Muhammad Jazzel Mehmood",
                    PhoneNumber = "+92-348-2453559",
                };
                var result = userManager.Create(user, "Jazz@123");
                if (result.Succeeded)
                {
                    userManager.AddToRole(user.Id, "Admin");
                }
            }
            if (!roleManger.RoleExists("Teacher"))
            {
                var role = new IdentityRole();
                role.Name = "Teacher";
                roleManger.Create(role);

                var user = new ApplicationUser()
                {
                    UserName = "JazzTeacher",
                    Email = "jazzelmehmood2013@gmail.com",
                    EmailConfirmed = true,
                    DateOfJoining = DateTime.Now,
                    Name = "Muhammad Jazzel Mehmood",
                    PhoneNumber = "+92-348-2453559",
                    CanvasAPI = "17361~ZRl7sKcI03MLwLA3nSBMLtZIJoYgM9Nr4D1ijHBBG1tuo2DNt4iEsnG5faHMJeBK",
                    InstructorProgram = "EE",
                };
                var result = userManager.Create(user, "Jazz@123");
                if (result.Succeeded)
                {
                    userManager.AddToRole(user.Id, "Teacher");
                }
            }
        }
    }
}