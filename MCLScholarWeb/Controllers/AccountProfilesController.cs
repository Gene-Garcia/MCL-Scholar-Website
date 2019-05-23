using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MCLScholarWeb.Models.Entity;

namespace MCLScholarWeb.Controllers
{
    public class UserProfilesController : Controller
    {
        private lipatdbEntities db = new lipatdbEntities();

        // GET: UserProfiles
        public ActionResult Index()
        {
            var StudentProfiles = db.StudentProfiles;
            return View(StudentProfiles.ToList());
        }

        // GET: UserProfiles/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StudentProfile UserProfile = db.StudentProfiles.Find(id);
            if (UserProfile == null)
            {
                return HttpNotFound();
            }
            return View(UserProfile);
        }

        // GET: UserProfiles/Create
        public ActionResult Create()
        {
            ViewBag.AccountID = new SelectList(db.AspNetUsers, "Id", "Email");
            return View();
        }

        // POST: UserProfiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.

        // GET: UserProfiles/Edit/5

        // POST: UserProfiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.


        // GET: UserProfiles/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser UserProfile = db.AspNetUsers.Find(id);
            if (UserProfile == null)
            {
                return HttpNotFound();
            }
            return View(UserProfile);
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
