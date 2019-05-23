using MCLScholarWeb.Models.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MCLScholarWeb.Controllers
{
    public class SettingsController : Controller
    {

        // GET: Settings
        private lipatdbEntities db;
        public SettingsController()
        {
            db = new lipatdbEntities();
        }
        public ActionResult Index()
        {
            ViewBag.Message = TempData["Message"];
            ViewBag.MessageType = TempData["MessageType"];
            var periods = db.ValidationPeriods.ToList();
            ViewBag.Periods = periods;
            return View(db.WebSettings.FirstOrDefault());
        }


        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult Index(WebSetting model)
        {
            var dbSetting = db.WebSettings.FirstOrDefault();
            var tempPeriod = db.ValidationPeriods.Where(e => e.PeriodID.Equals(model.PeriodID)).FirstOrDefault();
            var period = db.ValidationPeriods.Where(e => e.AcademicYearStart.Equals(tempPeriod.AcademicYearStart)&& e.AcademicYearEnd.Equals(tempPeriod.AcademicYearEnd)&& e.Term.Equals(model.ValidationPeriod.Term)).FirstOrDefault();
            if (period == null)
            {
                TempData["MessageType"] = "warning";
                TempData["Message"] = "Validation Period is not found";
                return RedirectToAction("Index");
            }
            dbSetting.ValidationPeriod = period;
            dbSetting.ValidationOpen = model.ValidationOpen;

            db.SaveChanges();

            ViewBag.MessageType = "success";
            ViewBag.Message = "Successfully changed website settings";
            ViewBag.Periods = db.ValidationPeriods.ToList();
            return View(db.WebSettings.FirstOrDefault());
        }


        public ActionResult AddPeriod()
        {
            return PartialView("_AddPeriod");
        }

        [HttpGet]
        public ActionResult GetTerms(int periodID)
        {
            var selectedPeriod = db.ValidationPeriods.Where(e => e.PeriodID.Equals(periodID)).FirstOrDefault();
            var terms = db.ValidationPeriods.Where(e => e.AcademicYearStart.Equals(selectedPeriod.AcademicYearStart) && e.AcademicYearEnd.Equals(selectedPeriod.AcademicYearEnd)).Select(e=>e.Term).ToList();
            return Json(terms, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]

        public ActionResult AddPeriod(ValidationPeriod period)
        {
            period.AcademicYearEnd = period.AcademicYearStart + 1;
            db.ValidationPeriods.Add(period);
            db.SaveChanges();
            TempData["Message"]="Period Successfully Added";
            TempData["MessageType"]="success";

            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult UpdateTerm(int year) {
            var term = db.ValidationPeriods.Select(e => e.Term).Distinct();
            return Json(term,JsonRequestBehavior.AllowGet);
        }


    }
}