using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CFMS.Classes;
using CFMS.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.Twitter.Messages;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;


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
                    List<object> listData = JsonConvert.DeserializeObject<List<object>>(responseData);

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
            courseFile.Status = "INITIALIZED";
            courseFile.Term = "Spring";
            courseFile.Title = "Canvas API";
            courseFile.CourseSection = "Section";


            db.CourseFiles.Add(courseFile);
            db.SaveChanges();

            return RedirectToAction("Folders", "Home", new { id = courseFile.Id });
        }

        public ActionResult Folders(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseFile courseFile = db.CourseFiles.Find(id);
            if (courseFile == null)
            {
                return HttpNotFound();
            }

            string rootDir = Server.MapPath("~/Data/");
            string folderPath = Path.Combine(rootDir, courseFile.CourseProgram, courseFile.Title, courseFile.Term, courseFile.CourseSection);
            string syllabusPath = Path.Combine(folderPath, "Course Syllabus");
            string objectivesPath = Path.Combine(folderPath, "Course Objectives");
            string contentPath = Path.Combine(folderPath, "Course Content");
            string weeklyPath = Path.Combine(folderPath, "Weekly Plan of Content of Lectures Delivered");
            string attendancePath = Path.Combine(folderPath, "Attendance Record");
            string lecPath = Path.Combine(folderPath, "Copy of Lecture Notes");
            string refPath = Path.Combine(folderPath, "List of Reference Material");
            string assignPath = Path.Combine(folderPath, "Copy of Assignment & Class Assessments");
            string modelAssignPath = Path.Combine(folderPath, "Model Solution of All Assignment & Class Assessment");
            string threeAssignPath = Path.Combine(folderPath, "Three Sample Graded assignments securing max, min and average marks");
            string quizzesPath = Path.Combine(assignPath, "Copy of all Quizzes");
            string modelQuizzesPath = Path.Combine(assignPath, "Model Solution of All Quizzes");
            string threeQuizzesPath = Path.Combine(folderPath, "Three Sample Graded Quizzes securing max, min and average marks");
            string midPath = Path.Combine(folderPath, "Copy of all midterm exams");
            string modelMidPath = Path.Combine(folderPath, "Model Solution of All mid-term exams");
            string threeMidPath = Path.Combine(folderPath, "Three Sample Mid-Term securing max, min and average marks");
            string finalPath = Path.Combine(folderPath, "Copy of all End Term exams");
            string modelFinalPath = Path.Combine(folderPath, "Model Solution of All Final-term exams");
            string threeFinalPath = Path.Combine(folderPath, "Three Sample End-Term securing max, min and average marks");
            string marksPath = Path.Combine(folderPath, "Marks distribution and grading Model");
            string instructorPath = Path.Combine(folderPath, "Instructor Feedback");
            string resultsPath = Path.Combine(folderPath, "Complete result of the course");
            string obeReportPath = Path.Combine(folderPath, "OBE Report");
            string cqiPath = Path.Combine(folderPath, "CQI");
            string recommendPath = Path.Combine(folderPath, "Recommendations");

            string labOutlinePath = Path.Combine(folderPath, "Lab Outline");
            string labPath = Path.Combine(folderPath, "Lab Manuals");
            string modelLabPath = Path.Combine(folderPath, "Lab Samples");
            string labMidPath = Path.Combine(folderPath, "Copy of all Lab Mid Term Exam");
            string modelLabMidPath = Path.Combine(folderPath, "Model Solution of All Lab Mid Term Exam");
            string threeLabMidPath = Path.Combine(folderPath, "Three Sample Graded All Lab Mid-Term securing max, min and average marks");
            string labFinalPath = Path.Combine(folderPath, "Copy of all End Term exams");
            string modelLabFinalPath = Path.Combine(folderPath, "Model Solution of All Final-term exams");
            string threeLabFinalPath = Path.Combine(folderPath, "Three Sample End-Term securing max, min and average marks");

            string labSyllabusPath = Path.Combine(folderPath, "Lab Syllabus");
            string labObjectivesPath = Path.Combine(folderPath, "Lab Objectives");
            string labContentPath = Path.Combine(folderPath, "Lab Content");
            string weeklyLabPath = Path.Combine(folderPath, "Weekly Plan of Content of Labs Delivered");
            string marksAndGradePath = Path.Combine(folderPath, "Marks distribution and grading Model");
            string outcomesPath = Path.Combine(folderPath, "Outcomes Assessment (OBE Analytics Report)");
            string designPath = Path.Combine(folderPath, "Design Skills Technique");
            string materialPath = Path.Combine(folderPath, "Copy of Material Given");

            string quizzesExamsPath = Path.Combine(folderPath, "Copy of all Quizzes & Exams");
            string modelQuizzesExamsPath = Path.Combine(folderPath, "Model Solution of All Quizzes & Exams");
            string threeQuizzesExamsPath = Path.Combine(folderPath, "Three Sample Graded Quizzes & Exams securing max, min and average marks");
            string evaluationPath = Path.Combine(folderPath, "Student Evaluation");



            if (courseFile.Status == "INITIALIZED")
            {
                Directory.CreateDirectory(folderPath);

                if (courseFile.CourseProgram == "CS")
                {
                    if (courseFile.CourseType == "Theory")
                    {
                        Directory.CreateDirectory(objectivesPath);
                        Directory.CreateDirectory(syllabusPath);
                        Directory.CreateDirectory(contentPath);
                        Directory.CreateDirectory(weeklyPath);
                        Directory.CreateDirectory(attendancePath);
                        Directory.CreateDirectory(lecPath);
                        Directory.CreateDirectory(refPath);
                        Directory.CreateDirectory(assignPath);
                        Directory.CreateDirectory(modelAssignPath);
                        Directory.CreateDirectory(threeAssignPath);
                        Directory.CreateDirectory(quizzesPath);
                        Directory.CreateDirectory(modelQuizzesPath);
                        Directory.CreateDirectory(threeQuizzesPath);
                        Directory.CreateDirectory(midPath);
                        Directory.CreateDirectory(modelMidPath);
                        Directory.CreateDirectory(threeMidPath);
                        Directory.CreateDirectory(finalPath);
                        Directory.CreateDirectory(modelFinalPath);
                        Directory.CreateDirectory(threeFinalPath);
                        Directory.CreateDirectory(marksPath);
                        Directory.CreateDirectory(instructorPath);
                        Directory.CreateDirectory(resultsPath);
                    }
                    else if (courseFile.CourseType == "Lab")
                    {
                        Directory.CreateDirectory(labSyllabusPath);
                        Directory.CreateDirectory(labObjectivesPath);
                        Directory.CreateDirectory(labContentPath);
                        Directory.CreateDirectory(weeklyLabPath);
                        Directory.CreateDirectory(attendancePath);
                        Directory.CreateDirectory(materialPath);
                        Directory.CreateDirectory(refPath);
                        Directory.CreateDirectory(assignPath);
                        Directory.CreateDirectory(modelAssignPath);
                        Directory.CreateDirectory(threeAssignPath);
                        Directory.CreateDirectory(marksAndGradePath);
                        Directory.CreateDirectory(outcomesPath);
                        Directory.CreateDirectory(designPath);
                    }

                }
                else if (courseFile.CourseProgram == "ECE")
                {
                    if (courseFile.CourseType == "Theory")
                    {
                        Directory.CreateDirectory(syllabusPath);
                        Directory.CreateDirectory(obeReportPath);
                        Directory.CreateDirectory(attendancePath);
                        Directory.CreateDirectory(lecPath);
                        Directory.CreateDirectory(assignPath);
                        Directory.CreateDirectory(modelAssignPath);
                        Directory.CreateDirectory(threeAssignPath);
                        Directory.CreateDirectory(quizzesPath);
                        Directory.CreateDirectory(modelQuizzesPath);
                        Directory.CreateDirectory(threeQuizzesPath);
                        Directory.CreateDirectory(midPath);
                        Directory.CreateDirectory(modelMidPath);
                        Directory.CreateDirectory(threeMidPath);
                        Directory.CreateDirectory(finalPath);
                        Directory.CreateDirectory(modelFinalPath);
                        Directory.CreateDirectory(threeFinalPath);
                        Directory.CreateDirectory(resultsPath);
                        Directory.CreateDirectory(cqiPath);
                        Directory.CreateDirectory(recommendPath);
                    }
                    else if (courseFile.CourseType == "Lab")
                    {
                        Directory.CreateDirectory(labOutlinePath);
                        Directory.CreateDirectory(obeReportPath);
                        Directory.CreateDirectory(attendancePath);
                        Directory.CreateDirectory(labPath);
                        Directory.CreateDirectory(modelLabPath);
                        Directory.CreateDirectory(labMidPath);
                        Directory.CreateDirectory(modelLabMidPath);
                        Directory.CreateDirectory(threeLabMidPath);
                        Directory.CreateDirectory(labFinalPath);
                        Directory.CreateDirectory(modelLabFinalPath);
                        Directory.CreateDirectory(threeLabFinalPath);
                        Directory.CreateDirectory(resultsPath);
                        Directory.CreateDirectory(cqiPath);
                        Directory.CreateDirectory(recommendPath);
                    }
                }
                else if (courseFile.CourseProgram == "ISciM")
                {
                    Directory.CreateDirectory(syllabusPath);
                    Directory.CreateDirectory(obeReportPath);
                    Directory.CreateDirectory(attendancePath);
                    Directory.CreateDirectory(lecPath);
                    Directory.CreateDirectory(assignPath);
                    Directory.CreateDirectory(modelAssignPath);
                    Directory.CreateDirectory(threeAssignPath);
                    Directory.CreateDirectory(quizzesPath);
                    Directory.CreateDirectory(modelQuizzesPath);
                    Directory.CreateDirectory(threeQuizzesPath);
                    Directory.CreateDirectory(midPath);
                    Directory.CreateDirectory(modelMidPath);
                    Directory.CreateDirectory(threeMidPath);
                    Directory.CreateDirectory(finalPath);
                    Directory.CreateDirectory(modelFinalPath);
                    Directory.CreateDirectory(threeFinalPath);
                    Directory.CreateDirectory(resultsPath);
                    Directory.CreateDirectory(cqiPath);
                    Directory.CreateDirectory(recommendPath);
                }

                else
                {
                    Directory.CreateDirectory(syllabusPath);
                    Directory.CreateDirectory(lecPath);
                    Directory.CreateDirectory(assignPath);
                    Directory.CreateDirectory(modelAssignPath);
                    Directory.CreateDirectory(threeAssignPath);
                    Directory.CreateDirectory(quizzesExamsPath);
                    Directory.CreateDirectory(modelQuizzesExamsPath);
                    Directory.CreateDirectory(threeQuizzesExamsPath);
                    Directory.CreateDirectory(attendancePath);
                    Directory.CreateDirectory(resultsPath);
                    Directory.CreateDirectory(evaluationPath);

                }
                courseFile.Status = "FETCHED";

                if (ModelState.IsValid)
                {
                    db.Entry(courseFile).State = EntityState.Modified;
                    db.SaveChanges();
                }

            }


            List<FolderInfo> folderInfos = new List<FolderInfo>();

            if (Directory.Exists(folderPath))
            {
                string[] subdirectories = Directory.GetDirectories(folderPath);
                foreach (string subdirectory in subdirectories)
                {
                    string folderName = Path.GetFileName(subdirectory);
                    string[] files = Directory.GetFiles(subdirectory);
                    List<string> fileNames = new List<string>();
                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileName(file);
                        fileNames.Add(fileName);
                    }
                    FolderInfo folderInfo = new FolderInfo(folderName, fileNames);
                    folderInfos.Add(folderInfo);
                }
            }

            ViewBag.folderInfos = folderInfos;
            return View(courseFile);

        }

        public ActionResult File(string courseId, string folder)
        {
            if (courseId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseFile courseFile = db.CourseFiles.Find(int.Parse(courseId));
            if (courseFile == null)
            {
                return HttpNotFound();
            }
            if (folder == null || folder == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            FolderInfo folderInfo;
            string rootDir = Server.MapPath("~/Data/");
            string folderPath = Path.Combine(rootDir, courseFile.CourseProgram, courseFile.Title, courseFile.Term, courseFile.CourseSection, folder);

            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath);
                List<string> fileNames = new List<string>();
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    fileNames.Add(fileName);
                }
                folderInfo = new FolderInfo(folder, fileNames);
                ViewBag.FolderInfo = folderInfo;
            }


            return View(courseFile);
        }

        public ActionResult DeleteFile(string courseId, string folder, string file)
        {
            if (courseId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseFile courseFile = db.CourseFiles.Find(int.Parse(courseId));
            if (courseFile == null)
            {
                return HttpNotFound();
            }
            if (folder == null || folder == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string rootDir = Server.MapPath("~/Data/");
            string folderPath = Path.Combine(rootDir, courseFile.CourseProgram, courseFile.Title, courseFile.Term, courseFile.CourseSection, folder);

            string fileToDelete = Path.Combine(folderPath, file);
            FileInfo fileObj = new FileInfo(fileToDelete);

            if (fileObj.Exists)
            {
                fileObj.Delete();
            }

            return RedirectToAction("File", new { courseId, folder });
        }

        [HttpPost]
        public ActionResult UploadFile(IEnumerable<HttpPostedFileBase> files, string courseId, string folder)
        {
            if (courseId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseFile courseFile = db.CourseFiles.Find(int.Parse(courseId));
            if (courseFile == null)
            {
                return HttpNotFound();
            }
            if (folder == null || folder == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string rootDir = Server.MapPath("~/Data/");
            string folderPath = Path.Combine(rootDir, courseFile.CourseProgram, courseFile.Title, courseFile.Term, courseFile.CourseSection, folder);

            if (files != null && files.Any())
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(file.FileName);

                        string filePath = Path.Combine(folderPath, fileName);
                        file.SaveAs(filePath);
                    }
                }
            }

            return RedirectToAction("File", new { courseId, folder });

        }

        public ActionResult CreateFolder(string courseId, string folder)
        {
            if (courseId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseFile courseFile = db.CourseFiles.Find(int.Parse(courseId));
            if (courseFile == null)
            {
                return HttpNotFound();
            }
            if (folder == null || folder == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string rootDir = Server.MapPath("~/Data/");
            string folderPath = Path.Combine(rootDir, courseFile.CourseProgram, courseFile.Title, courseFile.Term, courseFile.CourseSection, folder);

            Directory.CreateDirectory(folderPath);


            return RedirectToAction("Folders", new {id =  courseId});
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