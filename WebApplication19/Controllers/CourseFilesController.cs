using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CFMS.Models;

namespace CFMS.Controllers
{
    public class CourseFilesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: CourseFiles
        public ActionResult Index()
        {
            var courseFiles = db.CourseFiles.Include(c => c.ApplicationUser);
            return View(courseFiles.ToList());
        }

        // GET: CourseFiles/Details/5
        public ActionResult Details(int? id)
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
            return View(courseFile);
        }

        // GET: CourseFiles/Create
        public ActionResult Create()
        {
            ViewBag.ApplicationUserId = new SelectList(db.ApplicationUsers, "Id", "Name");
            return View();
        }

        // POST: CourseFiles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Term,Url,CourseProgram,CourseType,CourseSection,Status,ApplicationUserId,Created,Updated")] CourseFile courseFile)
        {
            if (ModelState.IsValid)
            {
                db.CourseFiles.Add(courseFile);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ApplicationUserId = new SelectList(db.ApplicationUsers, "Id", "Name", courseFile.ApplicationUserId);
            return View(courseFile);
        }

        // GET: CourseFiles/Edit/5
        public ActionResult Edit(int? id)
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
            ViewBag.ApplicationUserId = new SelectList(db.ApplicationUsers, "Id", "Name", courseFile.ApplicationUserId);
            return View(courseFile);
        }

        // POST: CourseFiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Term,Url,CourseProgram,CourseType,CourseSection,Status,ApplicationUserId,Created,Updated")] CourseFile courseFile)
        {
            if (ModelState.IsValid)
            {
                db.Entry(courseFile).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ApplicationUserId = new SelectList(db.ApplicationUsers, "Id", "Name", courseFile.ApplicationUserId);
            return View(courseFile);
        }

        // GET: CourseFiles/Delete/5
        public ActionResult Delete(int? id)
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
            return View(courseFile);
        }

        // POST: CourseFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CourseFile courseFile = db.CourseFiles.Find(id);
            db.CourseFiles.Remove(courseFile);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
