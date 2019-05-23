using MCLScholarWeb.Actions;
using MCLScholarWeb.Models.Entity;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

/*
 * Developer name: Gene Joseph Garcia
 * Module name: Website Content Management Module
 * Description: This controller will be responsible for displaying the announcement, downloadable forms, scholarsips, and list of scholars.
 * 
 * */

namespace MCLScholarWeb.Controllers
{
    public class WebsiteContentController : Controller
    {
        private WebsiteContentActions websiteActions;

        public WebsiteContentController()
        {
            websiteActions = new WebsiteContentActions();
        }

        /*
         * Displays all the announcements.
         * */
        [HttpGet]
        public ActionResult Announcement()
        {
            List<Announcement> announcement = null;
            try
            {
                ViewBag.errorMessage = TempData["errorMessage"];

                announcement = websiteActions.GetAnnouncements();
                return View(announcement);
            }
            catch (Exception e)
            {
                ViewBag.errorMessage = "Sorry there was an error in displaying the announcement. Please try again." + e.ToString();
                return View(announcement);
            }
        }

        /*
         * Displays all the available scholarships where the user can choose from and will be redirected to displaying its inforation.
         * */
        [HttpGet]
        public ActionResult ScholarshipSelection()
        {
            List<Scholarship> scholarships = null;

            try
            {
                ViewBag.errorMessage = TempData["errorMessage"];
                scholarships = websiteActions.GetAllScholarshipType();
                return View(scholarships);
            }
            catch (Exception e)
            {
                ViewBag.errorMessage = "Sorry there was an error in displaying the scholarships. Please try again later or contact our tech support.";
                return View(scholarships);
            }
        }

        /*
         * Displays the description of the selected scholarship.
         * */
        [HttpGet]
        public ActionResult Scholarship(int? scholarshipID)
        {
            Scholarship scholarship = null;
            try
            {
                if(scholarshipID == null)
                {
                    return RedirectToAction("ScholarshipSelection");
                }
                else
                {
                    scholarship = websiteActions.GetScholarshipByID( (int)scholarshipID );
                    return View(scholarship);
                }

            }
            catch(Exception e)
            {
                TempData["errorMessage"] = "Sorry there was an error in displaying the scholarships. Please try again.";
                return RedirectToAction("ScholarshipSelection");
            }
        }

        /*
         * Displays all the available scholarships where the user can choose from and will be redirected to displaying all the scholars of that scholarship.
         * */
        [HttpGet]
        public ActionResult ScholarSelection()
        {
            List<Scholarship> scholars = null;
            try
            {
                ViewBag.errorMessage = TempData["errorMessage"];
                scholars = websiteActions.GetAllScholarshipTypeThatAreListed();
                return View(scholars);
            }
            catch(Exception e)
            {
                ViewBag.errorMessage = "Sorry there was an error in displaying the list of scholars. Please try again.";
                return View(scholars);
            }
        }

        /*
         * Displays the list of scholars of the selected scholarship.
         * */
        [HttpGet]
        public ActionResult Scholars(int? scholarshipID)
        {
            List<PreValidatedStudent> scholarList = null;
            try
            {

                if(scholarshipID == null)
                {
                    return RedirectToAction("ScholarSelection");
                }
                else
                {
                    scholarList = websiteActions.GetScholarsByScholarshipID((int)scholarshipID)
                        .Where(model => model.PeriodID == WebSettings.CurrentPeriod().PeriodID)
                        .OrderBy(model => model.AspNetUser.UserProfiles.FirstOrDefault().StudentProfile.LastName)
                        .ToList();
                    if(scholarList.Count() > 0)
                    {
                        return View(scholarList);
                    }

                    TempData["errorMessage"] = "Sorry but there are no scholars for that scholarship.";
                    return RedirectToAction("ScholarSelection");
                }
            }
            catch (Exception e)
            {
                TempData["errorMessage"] = "Sorry there was an error in displaying the the list of scholars. Please try again.";
                return RedirectToAction("ScholarSelection");
            }
        }

        /*
         * Displays all the forms and their description.
         * */
        [HttpGet]
        public ActionResult DownloadableForms()
        {
            List<Form> forms = null;
            try
            {
                ViewBag.errorMessage = TempData["errorMessage"];
                forms = websiteActions.GetAllForms();
                return View(forms);
            }
            catch(Exception e)
            {
                ViewBag.errorMessage = "Sorry there was an error in displaying the forms. Please try again.";
                return View(forms);
            } 
        }

    }
}