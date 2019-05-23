using MCLScholarWeb.Actions;
using MCLScholarWeb.Models.Entity;
using MCLScholarWeb.Models.Report;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MCLScholarWeb.Controllers
{
    public class ReportsController : Controller
    {
        private lipatdbEntities db;
        public ReportsController()
        {
            db = new lipatdbEntities();
        }
        // GET: Reports

          

        public ActionResult Index()
        {
            ViewBag.Periods = db.ValidationPeriods.ToList();
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public JsonResult GetTerms(int periodID) {
            var selectedPeriod = db.ValidationPeriods.Where(e => e.PeriodID.Equals(periodID)).FirstOrDefault();
            var terms = db.ValidationPeriods.Where(e => e.AcademicYearStart.Equals(selectedPeriod.AcademicYearStart) && e.AcademicYearEnd.Equals(selectedPeriod.AcademicYearEnd)).Select(e => e.Term).ToList();
            return Json(terms, JsonRequestBehavior.AllowGet);
        }
        


        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult ReportPartial(int periodID,int term) {
            var tempPeriod = db.ValidationPeriods.Where(e => e.PeriodID.Equals(periodID)).FirstOrDefault();
            var period = db.ValidationPeriods.Where(e => e.AcademicYearStart.Equals(tempPeriod.AcademicYearStart)&&e.AcademicYearEnd.Equals(tempPeriod.AcademicYearEnd) && e.Term.Equals(term)).FirstOrDefault();
            if (period!= null)
            {
                ReportsMasterViewModel viewModel = new ReportsMasterViewModel()
                {
                    Scholarships = db.Scholarships.ToList(),
                    UserProfiles = db.UserProfiles.Where(e=>e.AspNetUser.ValidationPeriod.PeriodID.Equals(period.PeriodID)).ToList(),
                    ValidationPeriods = db.ValidationPeriods.ToList(),
                    ValidationRequests = db.ValidationRequests.Where(e=>e.PeriodID.Equals(period.PeriodID)).ToList()
                };

                var userID = User.Identity.GetUserId();
                AspNetUser user = db.AspNetUsers.Where(e=>e.Id.Equals(userID)).FirstOrDefault();
                ViewBag.PeriodID = periodID;
                string message = string.Format("Generated report for {0} to {1}, term {2}",period.AcademicYearStart, period.AcademicYearEnd, term);
                AuditActions.AddMessage(db,message,userID);
                return PartialView("_Report", viewModel);

            }
            else
            {
                return Content("<html>Report cannot be generated</html>");
            }
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult ReportPDF(int periodID, int term) {
            var period = db.ValidationPeriods.Where(e => e.PeriodID.Equals(periodID) && e.Term.Equals(term)).FirstOrDefault();
            if (period != null)

            {
                ReportsMasterViewModel viewModel = new ReportsMasterViewModel()
                {
                    Scholarships = db.Scholarships.ToList(),
                    UserProfiles = db.UserProfiles.Where(e => e.AspNetUser.ValidationPeriod.PeriodID.Equals(period.PeriodID)).ToList(),
                    ValidationPeriods = db.ValidationPeriods.ToList(),
                    ValidationRequests = db.ValidationRequests.Where(e => e.PeriodID.Equals(period.PeriodID)).ToList()
                };

                var userID = User.Identity.GetUserId();
                AspNetUser user = db.AspNetUsers.Where(e => e.Id.Equals(userID)).FirstOrDefault();
                ViewBag.PeriodID = periodID;
                string message = string.Format("Generated report for {0} to {1}, term {2}", period.AcademicYearStart, period.AcademicYearEnd, term);
                AuditActions.AddMessage(db, message, userID);
                return new Rotativa.PartialViewAsPdf("_Report", viewModel);
            }
            else {
                return Content("<html>Report cannot be generated</html>");
            }
        }

        [Authorize(Roles = "Administrator")]
        public List<int> getYears(int yearStart,int yearEnd) {
            List<int> years = new List<int>();
            db.ValidationPeriods.Where(e => e.AcademicYearStart.Equals(yearStart) && e.AcademicYearEnd.Equals(yearEnd)).ToList().ForEach(e => years.Add(e.Term));
            return years;
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Students()
        {
            var student = db.UserProfiles.ToList();
            return View(student);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Requests()
        {
            var requests = db.ValidationRequests.ToList();
            return View(requests);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Scholars()
        {
            var requests = db.ValidationRequests.Where(e=>e.ValidationStatu.StatusName.Equals("Approved")).ToList();
            return View();
        }


        public ActionResult Export()
        {
            return View();
        }
    }
}