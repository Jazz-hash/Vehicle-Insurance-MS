using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CFMS.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.Twitter.Messages;
using Newtonsoft.Json;

namespace CFMS.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly HttpClient httpClient;


        public HomeController()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            httpClient = new HttpClient();
            db = new ApplicationDbContext();
        }

        public HomeController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ActionResult Index()
        {
            //var data = from item in db.PolicyTypes
            //           where
            //           item.PolicyTypeId == 5 ||
            //           item.PolicyTypeId == 6 ||
            //           item.PolicyTypeId == 7
            //           select item;
            //dynamic HOMEMODEL = new ExpandoObject();
            //HOMEMODEL.PolicySetOne = db.PolicyTypes.Take(4);
            //HOMEMODEL.PolicySetTwo = data.ToList();
            //HOMEMODEL.PolicySetThree = db.PolicyTypes.OrderByDescending(x => x.PolicyTypeId).Take(4);
            return View(/*HOMEMODEL*/);
        }
        public ActionResult UserIndex()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public ActionResult Dashboard()
        {
            ViewBag.UserCount = db.Users.Count();
            ViewBag.CourseFilesLeft = 0;
            ViewBag.CourseFilesPending = 0;
            ViewBag.CourseFilesGenerated = 0;
            return View();
        }
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> TeachersDashboard()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewBag.userName = user.Name;
            ViewBag.instructorProgram = user.InstructorProgram;
            string API_URL = "https://hulms.instructure.com/api/v1/courses";
            string ACCESS_TOKEN = user.CanvasAPI;

            try
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + ACCESS_TOKEN);

                // Send the HTTP GET request
                HttpResponseMessage response = await httpClient.GetAsync(API_URL);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a string
                    string responseData = await response.Content.ReadAsStringAsync();
                    List<object>  listData = JsonConvert.DeserializeObject<List<object>>(responseData);

                    // TODO: Process the response data as
                    //ViewBag.data = jsonData[0];

                    return View(); // Return a view with the processed data
                }
                else
                {
                    // The API returned an error
                    string errorMessage = $"API request failed with status code: {response.StatusCode}";
                    // TODO: Handle the error appropriately, such as displaying an error message

                    return View("Error"); // Return an error view
                }
            }
            catch (Exception ex)
            {
                // An exception occurred while making the request
                string errorMessage = $"An error occurred while making the API request: {ex.Message}";
                // TODO: Handle the exception appropriately, such as logging or displaying an error message

                return View("Error"); // Return an error view
            }
        }
        [HttpPost]
        public ActionResult TeachersDashboard(CourseFile courseFile)
        {
            courseFile.ApplicationUserId = User.Identity.GetUserId();

            db.CourseFiles.Add(courseFile);
            db.SaveChanges();

            return RedirectToAction("Folders", "Home");
        }

        public ActionResult Folders()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Success = "";
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [HttpPost]
        public ActionResult Contact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                db.Contacts.Add(contact);
                db.SaveChanges();
                ViewBag.Success = "Done";
            }
            return View(contact);
        }
        public ActionResult Testimonial()
        {

            return View();
        }
        public ActionResult SiteMap()
        {

            return View();
        }

        public bool IsNumeric(string value)
        {
            return value.All(char.IsNumber);
        }
    }
}