using MCLScholarWeb.Actions;
using MCLScholarWeb.Models.Entity;
using MCLScholarWeb.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

/*
 * Developer name: Gene Joseph Garcia
 * Module name: Scholar Module for students.
 * Description: This controller will be responsible for viewing the scholarships of the student, validation requests, 
 * file a validation request, and edit a validation request. Furthermore, the student manual will also be available.
 * 
 * */

namespace MCLScholarWeb.Controllers
{
    public class StudentController : Controller
    {
        private StudentActions studentActions = new StudentActions();

        public StudentController()
        {
        }

        /*
         * Displays the student manual.
         * */
        public ActionResult StudentManual()
        {
            if (User.IsInRole("Student"))
            {
                return View();
            }
            else
            {
                TempData["errorMessage"] = "Sorry you do not have access.";
                return RedirectToAction("Announcement", "WebsiteContent");
            }
        }

        /*
         * Displays all the scholarships of the logged student in the current year and term.
         * */
        [HttpGet]
        public ActionResult StudentScholarship()
        {
            List<PreValidatedStudent> scholarships = null;
            try
            {
                if(User.IsInRole("Student"))
                {
                    ViewBag.IsValidationOpen = WebSettings.IsValidationOpen();
                    if (!WebSettings.IsValidationOpen())
                    {
                        ViewBag.ValidationMessage = "Scholarship validation is not yet open.";
                        return View();
                    }

                    ViewBag.errorMessage = TempData["errorMessage"];

                    ViewBag.NonListedScholarship = studentActions.GetDBObject().Scholarships.Where(
                            model => model.ScholarshipName.ToLower() == "sibling discount" ||
                            model.ScholarshipName.ToLower() == "ygc promotional discount" ||
                            model.ScholarshipName.ToLower() == "mcl study aid"
                        ).ToList();

                    scholarships = studentActions.GetAllScholars()
                        .Where(model => model.UserID == User.Identity.GetUserId()
                        && model.PeriodID == WebSettings.CurrentPeriod().PeriodID)
                        .Distinct()
                        .ToList();

                    return View(scholarships);
                }
                else
                {
                    TempData["errorMessage"] = "Sorry you do not have access.";
                    return RedirectToAction("Announcement","WebsiteContent"); //maybe use content with the navbar for easy navigation, no sense if redirected to announcement
                }
            }
            catch(Exception e)
            {
                ViewBag.errorMessage = "There was an error in displaying your scholarships. Please try again, thank you!";
                return View(scholarships);
            }
        }

        /*
         * Initiates on displaying the fields for the user to generate their own validation form. However, there are validations first.
         * if the student already has scholarship filled they cannot file another one. Only a financial aid or academic scholar and a voucher can
         * be validated by a student for the first time. 
         * */
        [HttpGet]
        public ActionResult ScholarshipValidationForm(int? scholarshipID)
        {
            try
            {
                if (User.IsInRole("Student"))
                {
                    if (scholarshipID == null)
                    {
                        return RedirectToAction("StudentScholarship");
                    }
                    else
                    {
                        ViewBag.AcademicYear = WebSettings.CurrentPeriod().AcademicYearStart + "-" + WebSettings.CurrentPeriod().AcademicYearEnd;
                        ViewBag.Term = WebSettings.CurrentPeriod().Term;
                        string userID = User.Identity.GetUserId();

                        //if student has passed a scholarship application form for that scholarship, dummy table
                        if (StudentActions.IsAVoucher(studentActions.GetScholarshipByID((int)scholarshipID)))
                        {
                            if (studentActions.IsScholarshipRequested( (int)scholarshipID, User.Identity.GetUserId(), 1, WebSettings.CurrentPeriod().PeriodID ))
                            {
                                TempData["errorMessage"] = "A voucher has already been requested. Please check the status.";
                                return RedirectToAction("StudentScholarship");
                            }
                            else
                            {
                                ViewBag.ScholarshipName = studentActions.GetScholarshipName((int)scholarshipID);
                                ViewBag.ScholarshipID = (int)scholarshipID;
                                return View(studentActions.GetDBObject().AspNetUsers.Where(model => model.Id == userID).FirstOrDefault());
                            }
                        }
                        else //not a voucher
                        {
                            if (studentActions.IsScholarshipRequested((int)scholarshipID, User.Identity.GetUserId(), 2, WebSettings.CurrentPeriod().PeriodID ))
                            {
                                TempData["errorMessage"] = "You already have ascholarship. Please check the status.";
                                return RedirectToAction("StudentScholarship");
                            }                       ///non listed meaning, walang listahan ng scholars.
                            else if (StudentActions.IsScholarshipNonListed(studentActions.GetScholarshipByID((int)scholarshipID).ScholarshipName.ToLower()))
                            {
                                ViewBag.ScholarshipName = studentActions.GetScholarshipName((int)scholarshipID);
                                ViewBag.ScholarshipID = (int)scholarshipID;
                                return View( studentActions.GetDBObject().AspNetUsers.Where(model => model.Id == userID).FirstOrDefault() );
                            }
                            else
                            {
                                ViewBag.ScholarshipName = studentActions.GetScholarshipName((int)scholarshipID);
                                ViewBag.ScholarshipID = (int)scholarshipID;
                                return View(studentActions.GetDBObject().AspNetUsers.Where(model => model.Id == userID).FirstOrDefault());
                            }
                        }
                    }
                }
                else
                {
                    TempData["errorMessage"] = "Sorry you do not have access.";
                    return RedirectToAction("Announcement", "WebsiteContent");
                }
            }
            catch (Exception e)
            {
                TempData["errorMessage"] = "There was an error. Please try again, thank you!" + e.ToString();
                return RedirectToAction("StudentScholarship");
            }
        }

        /*
         *  Called after generating a scholarship validation form. This action result will dispay the file upload where student uploads there
         *  requirements including the generateed scholarship validation form. This will also determine if the scholarship is a sibling discount
         *  if it is then a field for the parent name will be displayed where student must enter a value.
         *  And checks if the student already have an application form.
         * */
        [HttpGet]
        public ActionResult ValidateScholarship(int? scholarshipID)
        {
            try
            {
                if(User.IsInRole("Student"))
                {
                    if( scholarshipID == null)
                    {
                        return RedirectToAction("StudentScholarship");
                    }
                    else
                    {
                        bool isScholarshipSiblingDiscount = studentActions.GetScholarshipByID( (int)scholarshipID ).ScholarshipName.ToLower() == "sibling discount" ? true:false;
                        ViewBag.isScholarshipSiblingDiscount = isScholarshipSiblingDiscount;

                        bool isStudentPassedApplicationForm = studentActions.IsStudentPassedApplicationForm((int)scholarshipID, User.Identity.GetUserId());
                        ViewBag.isStudentPassedApplicationForm = isStudentPassedApplicationForm;

                        return View(new ValidationRequest() {
                            ScholarshipID = (int)scholarshipID,
                            DateFilled = DateTime.Now,
                            UserID = User.Identity.GetUserId(),
                            StatusID = 1, //1 == approved
                            PeriodID = WebSettings.CurrentPeriod().PeriodID,
                            Processed = false,
                            Remarks = " "
                        });
                    }
                }
                else
                {
                    TempData["errorMessage"] = "Sorry you do not have access.";
                    return RedirectToAction("Announcement", "WebsiteContent");
                }
            }
            catch(Exception e)
            {
                TempData["errorMessage"] = "There was an error. Please try again, thank you!" + e.ToString();
                return RedirectToAction("StudentScholarship");
            }
        }

        /*
         * Uploads the validation request to the database. If the parent names does contain values, meaning the scholarship is a 
         * sibling discount, then it will be uploaded to SiblingsDiscount table.
         * */
        [HttpPost]
        public ActionResult ValidateScholarship(ValidationRequest requestModel, HttpPostedFileBase[] scholarshipForms, string parentFirstName, string parentMiddleName, string parentLastName)
        {
            try
            {
                if (User.IsInRole("Student"))
                {
                    string firstNameParent = parentFirstName;
                    string middleNameParent = parentMiddleName;
                    string lastNameParent = parentLastName;

                    ModelState.Remove("parentFirstName");
                    ModelState.Remove("parentMiddleName");
                    ModelState.Remove("parentLastName");

                    if (ModelState.IsValid)
                    {
                        if (FileVerification.AreFilesPDF(scholarshipForms))
                        {
                            studentActions.UploadStudentRequest(requestModel);
                            AddRequestFilesToResource(requestModel, scholarshipForms);

                            if(firstNameParent != null && middleNameParent != null && lastNameParent != null)
                            {
                                studentActions.InsertParent(requestModel.RequestID, firstNameParent, middleNameParent, lastNameParent);
                            }

                            return RedirectToAction("RequestStatus");
                        }
                        else
                        {
                            ViewBag.errorMessage = "Please upload pdf files only. Please try again.";
                        }                       
                    }
                    else
                    {
                        ViewBag.errorMessage = "Invalid inputs. Please try again.";
                    }

                    bool isScholarshipSiblingDiscount = studentActions.GetScholarshipByID(requestModel.ScholarshipID).ScholarshipName.ToLower() == "sibling discount" ? true : false;
                    ViewBag.isScholarshipSiblingDiscount = isScholarshipSiblingDiscount;

                    bool isStudentPassedApplicationForm = studentActions.IsStudentPassedApplicationForm(requestModel.ScholarshipID, User.Identity.GetUserId());
                    ViewBag.isStudentPassedApplicationForm = isStudentPassedApplicationForm;

                    return View(requestModel); //fall-through
                }
                else
                {
                    TempData["errorMessage"] = "Sorry you do not have access.";
                    return RedirectToAction("Announcement", "WebsiteContent");
                }
            }
            catch (Exception e)
            {
                TempData["errorMessage"] = "There was an error. Please try again, thank you!" + e.ToString();
                return RedirectToAction("RequestStatus");
            }
        }

        /*
         * Displays all the validation request of the student of the currrent term and academic year.
         * */
        [HttpGet]
        public ActionResult RequestStatus()
        {
            try
            {
                if (User.IsInRole("Student"))
                {

                    if (!WebSettings.IsValidationOpen())
                    {
                        return RedirectToAction("StudentScholarship");
                    }

                    ViewBag.errorMessage = TempData["errorMessage"];
                    return View( studentActions.GetMyRequest(WebSettings.CurrentPeriod().PeriodID, User.Identity.GetUserId() ) );
                }
                else
                {
                    TempData["errorMessage"] = "Sorry you do not have access.";
                    return RedirectToAction("Announcement", "WebsiteContent");
                }

            }
            catch (Exception e)
            {
                ViewBag.errorMessage = "There was an error in displaying your validation requests. Please try again, thank you!" + e.ToString();
                return View();
            }
        }

        /*
         * Diplays the fields for editing the selected validation requests.
         * */
        [HttpGet]
        public ActionResult EditRequest(int? requestID)
        {
            try
            {           
                if (User.IsInRole("Student"))
                {
                    ViewBag.errorMessage = TempData["errorMessage"];
                    if (requestID == null)                  
                        return RedirectToAction("StudentScholarship");
                    else
                        return View(studentActions.GetRequestByID((int) requestID));
                }
                else
                {
                    TempData["errorMessage"] = "Sorry you do not have access.";
                    return RedirectToAction("Announcement", "WebsiteContent");
                }
            }
            catch (Exception e)
            {
                TempData["errorMessage"] = "There was an error in displaying the request you wish to edit. Please try again, thank you!";
                return RedirectToAction("RequestStatus");
            }
        }

        /*
         * Updates the database using the request model of the student, and uploads the documents, if there are any, to the database and azure
         * blob storage.
         * */
        [HttpPost]
        public ActionResult EditRequest(ValidationRequest requestModel, HttpPostedFileBase[] documents)
        {
            try
            {
                if (User.IsInRole("Student"))
                {
                    requestModel.DateFilled = DateTime.Now;
                    if (ModelState.IsValid)
                    {
                        if(documents.First() != null)
                        {
                            if (FileVerification.AreFilesPDF(documents))
                            {
                                studentActions.UpdateStudentRequest(requestModel);
                                AddRequestFilesToResource(requestModel, documents);
                                return RedirectToAction("RequestStatus");
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Please upload pdf files only. Please try again.";
                                return RedirectToAction("EditRequest", new { scholarshipID = requestModel.ScholarshipID });
                            }
                        }
                        else
                        {
                            studentActions.UpdateStudentRequest(requestModel);
                            return RedirectToAction("RequestStatus");
                        }
                    }
                    else
                    {
                        ViewBag.errorMessage = "Invalid inputs. Please try again.";
                        
                    }
                    return View(requestModel);
                }
                else
                {
                    TempData["errorMessage"] = "Sorry you do not have access.";
                    return RedirectToAction("Announcement", "WebsiteContent");
                }
            }
            catch (Exception e)
            {
                TempData["errorMessage"] = "There was an error in updating your request. Please try again, thank you!" + e.ToString();
                return RedirectToAction("RequestStatus");
            }
        }

        /*
         * Deletes the selected uploaded requiremensts from the database and azure blob storage.
         * */
        public ActionResult DeleteRequestBlob(string fileName, int? fileID)
        {
            try
            {
                if(User.IsInRole("Student"))
                {
                    if (fileName == null || fileID == null)
                    {
                        return RedirectToAction("RequestStatus");
                    }
                    else
                    {
                        if (studentActions.DeleteRequestBlob(fileName))
                        {
                            RequestResource requestResource = studentActions.GetRequestResourceByFileID((int)fileID);
                            studentActions.DeleteFromRequestResource(requestResource);

                            FileLocationStorage fileLocationStorage = studentActions.GetFileLocationStorageByFileID((int)fileID);
                            studentActions.DeleteFromFileLocationStorage(fileLocationStorage);

                            return RedirectToAction("EditRequest", new { requestID = requestResource.RequestID });
                        }
                        TempData["errorMessage"] = "There was an error in the server. Please try again.";
                        return RedirectToAction("RequestStatus");
                    }
                }
                else
                {
                    TempData["errorMessage"] = "Sorry you do not have access.";
                    return RedirectToAction("Announcement", "WebsiteContent");
                }
            }
            catch(Exception e)
            {
                TempData["errorMessage"] = "There was an error in deletings. Please try again, thank you!";
                return RedirectToAction("RequestStatus");
            }
        }
        
        /*
         * Handles the uploading of documents to database. 
         * */
        private void AddRequestFilesToResource(ValidationRequest requestModel, HttpPostedFileBase[] files)
        {
                foreach (var file in files)
                {
                    if(file != null)
                    {
                        //upload blob and get file name
                        //get file name
                        string fileName = studentActions.UploadBlobFile(file);

                        BlobModel blobModel = studentActions.GetAzureValidationReqObject()
                            .GetBlobs()
                            .Where(model => model.FileName == System.Web.HttpUtility.UrlPathEncode(fileName))
                            .FirstOrDefault();

                        blobModel.FileName = HttpUtility.UrlDecode(blobModel.FileName);

                        studentActions.UploadRequestToResource(requestModel, blobModel);
                    }
                }
        }

    }
}