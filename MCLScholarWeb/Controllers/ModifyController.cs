using MCLScholarWeb.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MCLScholarWeb.Controllers
{
    public class ModifyController : Controller
    {
        private lipatdbEntities db;
        public ModifyController()
        {
            db = new lipatdbEntities();
        }
        // GET: TEMPP
        public ActionResult Index()
        {
            return View();
        }

        /// ACCOUNTS
        /// [Authorize(Roles = "Administrator")]
        public ActionResult EditAccount()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult EditAccount(UserProfile profile)
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteAccount(string id)
        {
            try
            {
                var toDeleteProfile = new UserProfile { UserID = id };
                var toDeleteUser = new AspNetUser { Id = id };

                db.Configuration.AutoDetectChangesEnabled = false;
                db.UserProfiles.Remove(toDeleteProfile);
                db.AspNetUsers.Remove(toDeleteUser);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message);
            }
            finally
            {
                db.Configuration.AutoDetectChangesEnabled = true;
            }

            return RedirectToAction("Accounts");
        }


        // SCHOLARSHIPS
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public ActionResult ViewScholarship(int id)
        {
            Scholarship scholarship = db.Scholarships.Where(e => e.ScholarshipID.Equals(id)).FirstOrDefault();
            return View(scholarship);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult AddScholarship()
        {
            return View(new Scholarship());
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult AddScholarship(Scholarship type)
        {

            db.Scholarships.Add(type);
            db.SaveChanges();
            return RedirectToAction("Scholarships");
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult EditScholarship(Scholarship type)
        {
            Scholarship scholarship = db.Scholarships.Where(e => e.ScholarshipID.Equals(type.ScholarshipID)).FirstOrDefault();
            db.SaveChanges();
            return View(scholarship);
        }




    }
}