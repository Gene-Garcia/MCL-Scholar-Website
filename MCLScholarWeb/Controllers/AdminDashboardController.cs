using DASHBOARD.Models;
using MCLScholarWeb.Actions;
using MCLScholarWeb.Models.Entity;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

/*
 * Developer name: Gene Joseph Garcia
 * Module name: Admin Dashboard for admins
 * Description: This controller responsible for displaying data in graphical represnetation and charts.
 * Registered students per dept, shs or college
 * Validated and Invalidated Scholar Count
 * Academic and Non Academic Scholars
 * Number of Scholarships
 * */

namespace DASHBOARD.Controllers
{
    public class AdminDashboardController : Controller
    {
        lipatdbEntities dbObject = new lipatdbEntities();
        
        public ActionResult Index()
        {
            return View();
            var a = WebSettings.MinDate();
            var b = WebSettings.MaxDate();

            DashboardModel data = new DashboardModel();
            List<string> names = new List<string>();

            List<int> periodIDs = GetPeriodIDs(a, b);

            foreach (var periodID in periodIDs)
            {
                List<Scholarship> scholarships = dbObject.Scholarships.Where(model => model.PeriodID == periodID).ToList();

                foreach (var scholarship in scholarships)
                {
                    data.Scholarships++;
                    names.Add( scholarship.ScholarshipName );
                }

            }

            data.ScholarshipNames = names;

            return Content(data.ScholarshipNames.ToArray().ToString());
        }

        /*
         * This will set the defaukt value of the Dashboard if there no selected year and especially on first run.
         * The default year will be current academic year.
         * */
        public ActionResult Dashboard(string dateStart, string dateEnd)
        {
            DashboardModel data = new DashboardModel();

            data.EarliestDateStart = WebSettings.MinDate();
            data.LatestDateEnd = WebSettings.MaxDate();
                
            if (dateStart == null || dateEnd == null)
            {
                data.SelectedDateStart = data.EarliestDateStart;
                data.SelectedDateEnd = data.LatestDateEnd;
            }
            else
            {
                data.SelectedDateStart = dateStart;
                data.SelectedDateEnd = dateEnd;
            }

            List<int> periodIDs = GetPeriodIDs(data.SelectedDateStart, data.SelectedDateEnd);
            List<string> names = new List<string>();

            foreach (var periodID in periodIDs)
            {
                List<Scholarship> scholarships = dbObject.Scholarships.Where(model => model.PeriodID == periodID).ToList();

                foreach (var scholarship in scholarships)
                {
                    data.Scholarships++;
                    names.Add(scholarship.ScholarshipName);
                }

            }

            data.ScholarshipNames = names;

            return View(data);
        }

        private List<int> GetPeriodIDs(string dateStart, string dateEnd)
        {
            List<int> periodIDs;

            var dateTimeStart = Convert.ToDateTime(dateStart).Date;
            var dateTimeEnd = Convert.ToDateTime(dateEnd).Date;

            periodIDs = dbObject.ValidationPeriods.
                Where(model => DateTime.Compare((DateTime)model.Date, dateTimeStart) >= 0 && 
                DateTime.Compare((DateTime)model.Date, dateTimeEnd) <= 0)
                .Select(model => model.PeriodID).
                ToList();

            return periodIDs;
        }
        
        /*
         * A line chart the uses the academic year as a label and count of validated and invalidated students, individually.
         * */
        public ContentResult GetValidationCount(string dateStart, string dateEnd)
        {
            List<DashboardModel> datas = new List<DashboardModel>();

            string parsedStartYear = dateStart;
            string parsedEndYear = dateEnd;

            List<int> periodIDs = GetPeriodIDs(dateStart, dateEnd);
            int approvedStatusID = dbObject.ValidationStatus.
                Where(model => model.StatusName.ToLower() == "approved").
                Select(model => model.StatusID).
                FirstOrDefault();

            foreach (var periodID in periodIDs)
            {
                DashboardModel data = new DashboardModel();

                DateTime date = (DateTime)dbObject.ValidationPeriods.
                    Where(model => model.PeriodID == periodID).Select(model => model.Date).FirstOrDefault();

                data.YearsSelected = date.ToString("yyyy-MM-dd");

                data.ValidatedStudents += dbObject.ValidationRequests.Where(model => model.StatusID == approvedStatusID && model.PeriodID == periodID).Count();

                data.NotValidatedStudents += dbObject.ValidationRequests.Where(model => model.StatusID != approvedStatusID && model.PeriodID == periodID).Count();

                datas.Add(data);
            }

            return Content(JsonConvert.SerializeObject(datas), "application/json");
        }

        /*
         * A bar graph that uses the count of academic and non academic scholar of the selected year.
         * */
        public ContentResult GetAcademicAndNonScholarCount(string dateStart, string dateEnd)
        {
            DashboardModel data = new DashboardModel();

            List<int> academicScholarshipIDs = dbObject.Scholarships.
                Where(model => model.ScholarshipType.ScholarshipTypeName.ToLower() == "academic scholarship").
                Select(model => model.ScholarshipID).ToList();
            List<int> nonAcademicScholarshipIDs = dbObject.Scholarships.
                Where(model => model.ScholarshipType.ScholarshipTypeName.ToLower() != "academic scholarship").
                Select(model => model.ScholarshipID).ToList();

            List<int> periodIDs = GetPeriodIDs(dateStart, dateEnd);

            foreach (var periodID in periodIDs)
            {
                List<PreValidatedStudent> scholarlists = dbObject.PreValidatedStudents.
                    Where(model => model.PeriodID == periodID).
                    ToList();

                if(scholarlists != null)
                {
                    foreach (var scholarshipId in academicScholarshipIDs)
                    {
                        data.AcademicScholars += scholarlists.
                            Where(model => model.ScholarshipID == scholarshipId).
                            Count();
                    }

                    foreach (var nonAcademicScholarshipID in nonAcademicScholarshipIDs)
                    {
                        data.NonAcademicScholars += scholarlists.
                            Where(model => model.ScholarshipID == nonAcademicScholarshipID).
                            Count();
                    }
                }
            }

            return Content(JsonConvert.SerializeObject(data), "application/json");
        }

        /*
         * A pie chart that uses the college and shs department count of registered students and the total number of registered students.
         * */
        public ContentResult GetRegisteredStudentCount(string dateStart, string dateEnd)
        {
            DashboardModel data = new DashboardModel();

            List<int> periodIDs = GetPeriodIDs(dateStart, dateEnd);

            foreach (var periodID in periodIDs)
            {
                data.RegisteredStudents += dbObject.AspNetUsers.
                    Where(model => model.PeriodID == periodID && 
                    model.AspNetUserRoles.FirstOrDefault().AspNetRole.Name.ToLower() == "student").
                    Count();

                data.SHSStudents += dbObject.AspNetUsers.
                    Where(model => model.UserProfiles.FirstOrDefault().StudentProfile.Department.ToLower() == "shs" && 
                    model.PeriodID == periodID && 
                    model.AspNetUserRoles.FirstOrDefault().AspNetRole.Name.ToLower() == "student").
                    Count();

                data.CollegeStudents += dbObject.AspNetUsers.
                    Where(model => model.UserProfiles.FirstOrDefault().StudentProfile.Department.ToLower() == "college" && 
                    model.PeriodID == periodID && 
                    model.AspNetUserRoles.FirstOrDefault().AspNetRole.Name.ToLower() == "student").
                    Count();
            }

            return Content(JsonConvert.SerializeObject(data), "application/json");
        }

    }
}