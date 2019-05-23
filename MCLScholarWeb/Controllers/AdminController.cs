
using MCLScholarWeb.Actions;
using MCLScholarWeb.Models.Entity;
using MCLScholarWeb.Models.Entity.Admin;
using MCLScholarWeb.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using Microsoft.AspNet.Identity;

namespace MCLScholarWeb.Controllers
{
    /// <summary>
    /// Created by: Job Lipat
    /// Date Created: 
    /// Purpose: 
    /// </summary>
    public class AdminController : Controller
    {
        private lipatdbEntities db;
        private ExcelActions ExcelActions;
        private AdminService service;
        private WebsiteContentActions actions2; //added by gene
        public AdminController()
        {
            db = new lipatdbEntities();
            service = new AdminService(db);
            ExcelActions = new ExcelActions();
            actions2 = new WebsiteContentActions();
        }
        // GET: Admin
        // [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AdminManual()
        {

            return View();
        }

        // ACCOUNT MANAGEMENT


        [AllowAnonymous]
        public ActionResult Accounts(string role)
        {
            ViewBag.Role = role;
            //validations
            ViewBag.Message = TempData["Message"];
            ViewBag.MessageType = TempData["MessageType"];
            var users = db.AspNetUsers.Where(e => e.AspNetUserRoles.FirstOrDefault().AspNetRole.Name.Equals(role));
            return View(users);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult EditAccount()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
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




        // END OF ACCOUNT MANAGEMENT




        // UPLOAD SCHOLAR LIST

        public ActionResult UploadList()
        {
            ViewBag.HasMessage = false;
            if (TempData["Message"] != null)
            {
                ViewBag.HasMessage = true;
                ViewBag.Message = TempData["Message"];
            }
            return View();
        }

        [HttpPost]
        public ActionResult _ReviewList(HttpPostedFileBase file)
        {
            List<PreValidatedStudent> scholars = new List<PreValidatedStudent>();
            var excel = new ExcelPackage(file.InputStream);
            ExcelWorksheet worksheet = excel.Workbook.Worksheets[1];
            //validation
            for (int i = 12; i <= worksheet.Dimension.End.Row; i++)
            {
                //validation

                //scholarhsip type, student id
                //loop all columns in a row
                string studentID = worksheet.Cells[i, 4].Value.ToString();
                int scholarshipID = Convert.ToInt32(worksheet.Cells[i, 5].Value);
                Scholarship type = db.Scholarships.Where(e => e.ScholarshipID.Equals(scholarshipID)).FirstOrDefault();
                ValidationPeriod period = db.WebSettings.FirstOrDefault().ValidationPeriod;
                AspNetUser user = db.AspNetUsers.Where(e => e.UserProfiles.FirstOrDefault().StudentProfile.StudentID.Equals(studentID)).FirstOrDefault();
                PreValidatedStudent tempScholar = new PreValidatedStudent();


                tempScholar.ScholarshipID = type.ScholarshipID;
                tempScholar.PeriodID = period.PeriodID;
                tempScholar.UserID = user.Id;
                tempScholar.Scholarship = type;
                tempScholar.ValidationPeriod = period;
                tempScholar.AspNetUser = user;

                scholars.Add(tempScholar);
  
            }
            TempData["Scholars"]=scholars;
            return PartialView("_ReviewList", scholars);
        }




        [AllowAnonymous]
        [HttpPost]
        public ActionResult UploadList(List<PreValidatedStudent> scholars)
        {
            scholars = (List<PreValidatedStudent>)TempData["Scholars"];

            int counter = 0;
            foreach (var item in scholars)
            {
                counter++;
                Scholarship type = db.Scholarships.Where(e => e.ScholarshipID.Equals(item.ScholarshipID)).FirstOrDefault();
                ValidationPeriod period = db.WebSettings.FirstOrDefault().ValidationPeriod;
                AspNetUser user = db.AspNetUsers.Where(e => e.Id.Equals(item.UserID)).FirstOrDefault();

                item.Scholarship = type;
                item.ValidationPeriod = period;
                item.AspNetUser = user;
                db.PreValidatedStudents.Add(item);
            }
            db.SaveChanges();
            //redirect to select scholarship
            TempData["Message"] = string.Format("Scholars Successfuly Added. Number of added scholars {0}", counter);
            return RedirectToAction("UploadList", "Admin");
        }


        public FileResult DownloadTemplate()
        {
            string filename = "UploadScholarsTemplate";
            return File(ExcelActions.GenerateTemplate().GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename + ".xlsx");
        }


        // END OF UPLOAD SCHOLAR LIST

        // SCHOLARSHIP
        [Authorize(Roles = "Administrator")]
        public ActionResult Scholarships()
        {
            ViewBag.Message = TempData["Message"];
            ViewBag.MessageType = TempData["MessageType"];
            List<Scholarship> scholarships = db.Scholarships.ToList();
            List<SelectListItem> items = new List<SelectListItem>();
            db.ScholarshipTypes.ToList().ForEach(e => items.Add(new SelectListItem
            {
                Text = e.ScholarshipTypeName,
                Value = e.ScholarshipTypeID.ToString()
            }));
            ViewBag.Types = items;

            return View(scholarships);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult ViewScholars(int id)
        {
            var scholars = db.ValidationRequests.Where(e => e.ValidationStatu.StatusName.Equals("Approved")).Where(e => e.ScholarshipID.Equals(id));
            ViewBag.ScholarshipName = db.Scholarships.Where(e => e.ScholarshipID.Equals(id)).First().ScholarshipName;
            return PartialView("_ViewScholars",scholars);
        }

        // END OF SCHOLARSHIP

        // START OF AUDIT LOG
        [Authorize(Roles = "Administrator")]
        public ActionResult AuditLog()
        {
            return View(db.AdminAuditLogs);
        }

        //END OF AUDIT LOG


        [Authorize(Roles = "Administrator")]
        public ActionResult Forms()
        {
            ViewBag.Message = TempData["Message"];
            ViewBag.MessageType = TempData["MessageType"];
            return View(db.Forms.ToList());
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Announcements()
        {
            ViewBag.Message = TempData["Message"];
            ViewBag.MessageType = TempData["MessageType"];
            return View(db.Announcements);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Documentation() 
        {
            return View();
        }

        /// ACCOUNTS
    

        // SCHOLARSHIPS
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public ActionResult ViewScholarship(int id)
        {
            Scholarship scholarship = db.Scholarships.Where(e => e.ScholarshipID.Equals(id)).FirstOrDefault();
            return View(scholarship);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult _CreateScholarship()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            db.ScholarshipTypes.ToList().ForEach(e=>items.Add(new SelectListItem {
                Text=e.ScholarshipTypeName,
                Value=e.ScholarshipTypeID.ToString()
            }));
            ViewBag.Types = items;
            return PartialView("_CreateScholarship",new Scholarship());
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult _CreateScholarship(Scholarship scholarship)
        {
            scholarship.ValidationPeriod = db.WebSettings.FirstOrDefault().ValidationPeriod;
            db.Scholarships.Add(scholarship);
            db.SaveChanges();
            return RedirectToAction("Scholarships", "Admin");
        }
        public ActionResult EditScholarship(int id) {
            return PartialView("_EditScholarship",db.Scholarships.Where(e=>e.ScholarshipID.Equals(id)).FirstOrDefault());
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult EditScholarship(Scholarship model)
        {
            Scholarship scholarship = db.Scholarships.Where(e => e.ScholarshipID.Equals(model.ScholarshipID)).FirstOrDefault();
            scholarship.ScholarshipName = model.ScholarshipName;
            scholarship.ScholarshipDescription = model.ScholarshipDescription;
            scholarship.ApplicationForm = model.ApplicationForm;
            scholarship.ScholarshipTypeID = model.ScholarshipTypeID;
            db.SaveChanges();
            TempData["Message"] = "Changes saved successfully";
            TempData["MessageType"] = "success";

            return RedirectToAction("Scholarships","Admin");
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteScholarship(int id)
        {
            Scholarship scholarship = db.Scholarships.Where(e => e.ScholarshipID.Equals(id)).FirstOrDefault();
            db.Scholarships.Remove(scholarship);
            db.SaveChanges();
            return RedirectToAction("Scholarships");
        }



    }
}



