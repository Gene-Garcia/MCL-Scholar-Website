using MCLScholarWeb.Actions;
using MCLScholarWeb.Storage;
using MCLScholarWeb.Models.Entity;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

/*
 * Developer name: Gene Joseph Garcia
 * Module name: Website Content Management Module for admins
 * Description: This controller will be responsible for initiating the editing, uploading/posting, deleting/archiving of 
 * announcements, forms, and details.
 * 
 * */

namespace MCLScholarWeb.Controllers
{
    public class AdminContentController : Controller
    {
        private WebsiteContentActions websiteActions;

        public AdminContentController()
        {
            websiteActions = new WebsiteContentActions();

        }

        // A N N O U N C E M E N T S \\

        /*
         * Displays the field for all posting an announcement.
         * */
        [HttpGet]
        public ActionResult PostAnnouncement()
        {
            try
            {
                if (User.IsInRole("Administrator"))
                {
                    return View();
                }
                else
                {
                    TempData["errorMessage"] = "Sorry you do not have access.";
                    return RedirectToAction("Announcements", "Admin");
                }
            }
            catch (Exception e)
            {
                TempData["errorMessage"] = "Sorry there was an error." + e.ToString();
                return RedirectToAction("Announcement", "WebsiteContent");
            }
        }

        /*
         * Uploads the announcement model and photos, if there is any, to the database.
         * */
        [HttpPost]
        public ActionResult PostAnnouncement(Announcement announcementModel, HttpPostedFileBase[] photos)
        {
            try
            {
                if (User.IsInRole("Administrator"))
                {
                    if (ModelState.IsValid && (FileVerification.AreFilesPhoto(photos) == "photo" || FileVerification.AreFilesPhoto(photos) == "null"))
                    {
                        announcementModel.UserID = User.Identity.GetUserId();
                        announcementModel.Status = "Active";
                        announcementModel.DateAdded = DateTime.Now;

                        websiteActions.UploadAnnouncement(announcementModel);
                        UploadAnnouncementToDatabase(announcementModel, photos);

                        return RedirectToAction("Announcements", "Admin");
                    }
                    else
                    {
                        ViewBag.errorMessage = "Please enter an image file only.";
                        return View(announcementModel);
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
                ViewBag.errorMessage = "There was an error in posting the announcement. Please try again, thank you!";
                return View(announcementModel);
            }
        }

        /*
         * Displays the field for all editing an announcement selected by the admin.
         * */
        [HttpGet]
        public ActionResult EditAnnouncement(int? announcementID)
        {
            try
            {
                if (announcementID == null)
                {
                    return RedirectToAction("Announcement", "WebsiteContent");
                }
                else
                {
                    if (User.IsInRole("Administrator"))
                    {
                        ViewBag.errorMessage = TempData["errorMessage"];
                        return PartialView("_EditAnnouncement",websiteActions.GetAnnouncementByID((int)announcementID));
                    }
                    else
                    {
                        TempData["errorMessage"] = "Sorry you do not have access.";
                        return RedirectToAction("Announcement", "WebsiteContent");
                    }                    
                }
            }
            catch (Exception e)
            {
                TempData["errorMessage"] = "There was a error in updating this announcement. Please try again, thank you!";
                return RedirectToAction("Announcement", "WebsiteContent");
            }
        }

        /*
         * updates the announcement model and upload photos, if there is any, to the database.
         * */
        [HttpPost]
        public ActionResult EditAnnouncement(Announcement announcementModel, HttpPostedFileBase[] photos)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (FileVerification.AreFilesPhoto(photos) == "not")
                    {
                        TempData["errorMessage"] = "Please upload image files only.";
                        return RedirectToAction("EditAnnouncement", new { announcementID = announcementModel.AnnouncementID });
                    }
                    else
                    {
                        websiteActions.UpdateAnnouncement(announcementModel);
                        UploadAnnouncementToDatabase(announcementModel, photos);
                        return RedirectToAction("Announcements", "Admin");
                    }
                }
                else
                {
                    return View(announcementModel);
                }
            }
            catch (Exception e)
            {
                ViewBag.errorMessage = "Sorry there was an erro in editing this announcement, please try again." + e.ToString();
                return View(announcementModel);
            }

        }

        /*
         * Sets the Status of an announcement to Archived and updates it to the database.
         * */
        public ActionResult ArchiveAnnouncement(int? announcementID) /////////////////////////////////add if admin then do it else redirect
        {
            try
            {
                if (announcementID == null)
                {
                    return RedirectToAction("Announcement", "WebsiteContent");
                }
                else
                {
                    if (User.IsInRole("Administrator"))
                    {
                        Announcement announcement = websiteActions.GetAnnouncementByID((int)announcementID);
                        announcement.Status = "Archived";
                        websiteActions.UpdateAnnouncement(announcement);
                        return RedirectToAction("Announcements", "Admin");
                    }
                    else
                    {
                        TempData["errorMessage"] = "Sorry you do not have access.";
                        return RedirectToAction("Announcement", "WebsiteContent");
                    }
                    
                }
            }
            catch (Exception e)
            {
                TempData["errorMessage"] = "Sorry there was an error in archiving this announcement. Please try again, thank you!";
                return RedirectToAction("EditAnnouncement", new { announcementID = (int)announcementID });
            }
        }

        /*
         * Deletes the selected the image of an announcement and deletes it from database and the azure blob storage.
         * */
        public ActionResult RemoveAnnouncementBlob(string file, int? fileID)
        {
            try
            {
                if (User.IsInRole("Administrator"))
                {

                    if (file == null || fileID == null)
                    {
                        return RedirectToAction("Announcement", "WebsiteContent");
                    }
                    else
                    {
                        if (websiteActions.DeleteAnnouncementFile(file)) //para kung hindi nabura yung image, hindi na buburahin sa database
                        {
                            AnnouncementResource announcementResource = websiteActions.GetAnnouncementResourceByFileID((int)fileID);
                            websiteActions.DeleteFromAnnouncementResources(announcementResource);

                            FileLocationStorage fileLocationStorage = websiteActions.GetFileLocationStorageByFileID((int)fileID);
                            websiteActions.DeleteFromFileLocationStorage(fileLocationStorage);
                            return RedirectToAction("Announcements", "Admin");
                        }
                        else
                        {
                            TempData["errorMessage"] = "There was an error in the server. Please try again";
                            return RedirectToAction("Announcement", "WebsiteContent");
                        }
                        //actions2.DeleteAnnouncementFile(file); 
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
                TempData["errorMessage"] = "There was an error in removing the photo you have selected.";
                return RedirectToAction("Announcement", "WebsiteContent");
            }

        }

        /*
         * Handles the uploading of photos to azure blob storage and announcement resource.
         * */
        private void UploadAnnouncementToDatabase(Announcement announcementModel, HttpPostedFileBase[] photos)
        {
            if (photos.Length >= 1)
            {
                foreach (var photo in photos)
                {
                    if (photo != null)
                    {
                        string fileName = websiteActions.UploadAnnouncementBlobFile(photo);
                        string urlFileName = System.Web.HttpUtility.UrlPathEncode(fileName);
                        BlobModel blobModel = websiteActions.getAzureAnnouncementsObject().GetBlobs().Where(blob => blob.FileName == urlFileName).FirstOrDefault();
                        blobModel.FileName = HttpUtility.UrlDecode(blobModel.FileName);
                        websiteActions.InsertPhotoToAnnouncementResources(announcementModel, blobModel);

                    }
                }
            }
        }

        // D O W N L O A D A B L E   F O R M S \\

        /*
         * Displays the field for all posting a form and a description.
         * */
        [HttpGet]
        public ActionResult UploadForm()
        {
            try
            {
                if (User.IsInRole("Administrator"))
                {
                    return View();
                }
                else
                {
                    TempData["errorMessage"] = "Sorry you do not have access.";
                    return RedirectToAction("Forms", "Admin");
                }
            }
            catch (Exception e)
            {
                return Content(e.ToString());
            }
        }

        /*
         * Uploads the form model and the actual form to the database.
         * */
        [HttpPost]
        public ActionResult UploadForm(Form formModel, HttpPostedFileBase formFile)
        {
            try
            {
                formModel.UserID = User.Identity.GetUserId();
                formModel.DateAdded = DateTime.Now;

                if (ModelState.IsValid)
                {
                    if (formFile != null)
                    {
                        if (FileVerification.IsFileAPDF(formFile) == "not")
                        {
                            ViewBag.errorMessage = "Please upload pdf file only.";
                            return View(formModel);
                        }

                        websiteActions.UploadForm(formModel);

                        string fileName = websiteActions.UploadFormBlobFile(formFile);
                        string urlFileName = System.Web.HttpUtility.UrlPathEncode(fileName);

                        BlobModel blobModel = websiteActions.getAzureFormsObject().GetBlobs().Where(blob => blob.FileName == urlFileName).FirstOrDefault();
                        blobModel.FileName = HttpUtility.UrlDecode(blobModel.FileName);
                        websiteActions.InsertFormToFormResource(formModel, blobModel);
                        return RedirectToAction("Forms", "Admin");
                    }
                    else
                    {
                        ViewBag.errorMessage = "Please upload a file.";
                        return View(formModel);
                    }
                }
                else
                {
                    return View(formModel);
                }
            }
            catch (Exception e)
            {
                ViewBag.errorMessage = "There was an error in uploading your form, please try again.";
                return View(formModel);
            }
        }

        /*
         * Displays the fields of the selected upload downloadable form for editing.
         * */
        [HttpGet]
        public ActionResult EditForm(int? formID)
        {
        
                if (User.IsInRole("Administrator"))
                {

                    if (formID == null)
                    {
                        return RedirectToAction("DownloadableForms", "WebsiteContent");
                    }

                    return PartialView("_EditForm", websiteActions.GetFormByID((int)formID));
                }
                else
                {
                    TempData["errorMessage"] = "Sorry you do not have access.";
                    return RedirectToAction("DownloadableForms", "WebsiteContent");
                }
        
        }

        /*
         * Updates the database using the form model and uploads the new file, if there is any.
         * */
        [HttpPost]
        public ActionResult EditForm(Form formModel, HttpPostedFileBase formFile)
        {
        
                formModel.DateAdded = DateTime.Now;
                if (ModelState.IsValid)
                {
                    if (FileVerification.IsFileAPDF(formFile) == "not")
                    {
                        ViewBag.errorMessage = "Please upload pdf file only.";
                        return View(formModel);
                    }
                    else
                    {
                        websiteActions.UpdateForm(formModel);

                        if (formFile != null)
                        {
                            string fileName = websiteActions.UploadFormBlobFile(formFile);

                            string urlFileName = System.Web.HttpUtility.UrlPathEncode(fileName);

                            BlobModel blobModel = websiteActions.getAzureFormsObject().GetBlobs().Where(blob => blob.FileName == urlFileName).FirstOrDefault();
                            blobModel.FileName = HttpUtility.UrlDecode(blobModel.FileName);
                            websiteActions.InsertFormToFormResource(formModel, blobModel);
                        }

                        return RedirectToAction("Forms", "Admin");
                    }
                }
                else
                {
                    return View(formModel);
                }
            
        }

        /*
         * Removes a selected a file from an uploaded downloadable form. And deletes from the database.
         * */
        public ActionResult RemoveFormBlob(string fileName, int? fileID)
        {
            try
            {
                if (User.IsInRole("Administrator"))
                {

                    if (fileName == null || fileID == null)
                    {
                        return RedirectToAction("DownloadableForms");
                    }
                    else
                    {
                        if (websiteActions.DeleteFormFile(fileName))
                        {
                            FormsResource formResource = websiteActions.GetFormResourceByFileID((int)fileID);
                            websiteActions.DeleteFromFormResource(formResource);

                            FileLocationStorage fileLocationStorage = websiteActions.GetFileLocationStorageByFileID((int)fileID);
                            websiteActions.DeleteFromFileLocationStorage(fileLocationStorage);

                            return RedirectToAction("Forms", "Admin");
                        }
                        TempData["errorMessage"] = "There was an error in the server. Please try again.";
                        return RedirectToAction("Forms", "Admin");
                    }
                }
                else
                {
                    TempData["errorMessage"] = "Sorry you do not have access.";
                    return RedirectToAction("DownloadableForms", "WebsiteContent");
                }
            }
            catch (Exception e)
            {
                TempData["errorMessage"] = "There was an error in deleting the form";
                return RedirectToAction("DownloadableForms", "WebsiteContent");
            }
        }

        /*
         * Deletes the whole downloadable form from the database including the file.
         * */
        public ActionResult DeleteForm(int? formID)
        {
            try
            {
                if (User.IsInRole("Administrator"))
                {
                    if (formID == null)
                    {
                        return RedirectToAction("DownloadableForms", "WebsiteContent");
                    }
                    else
                    {
                        Form form = websiteActions.GetFormByID((int)formID);
                        FormsResource formResource = form.FormsResources.Where(model => model.FormID == formID).FirstOrDefault();
                        FileLocationStorage fileLocationStorage = websiteActions.GetFileLocationStorageByFileID(formResource.FileID);

                        if (websiteActions.DeleteFormFile(fileLocationStorage.FileName))
                        {
                            websiteActions.DeleteFromFormResource(formResource);
                            websiteActions.DeleteFromFileLocationStorage(fileLocationStorage);
                            websiteActions.DeleteFromForms(form);
                        }
                        return RedirectToAction("Forms", "Admin");
                    }
                }
                else
                {
                    TempData["errorMessage"] = "Sorry you do not have access.";
                    return RedirectToAction("DownloadableForm", "WebsiteContent");
                }
            }
            catch (Exception e)
            {
                TempData["errorMessage"] = "There was an error in deleting the form.";
                return RedirectToAction("DownloadableForms", "WebsiteContent");
            }
        }

        /*
         * Handles the downloading of a file. Because there are browsers that does not automatically downloads file, such as microsoft edge.
         * */ //lipat mo nalang sa website content
        public ActionResult DownloadForm(string fileName)
        {

            try
            {
                if (fileName == null)
                {
                    return RedirectToAction("DownloadableForms", "WebsiteContent");
                }
                else
                {
                    MemoryStream ms = new MemoryStream();

                    var blob = websiteActions.getAzureFormsObject().GetCloudBlobByFileReferece(HttpUtility.UrlDecode(fileName));

                    blob.DownloadToStreamAsync(ms);
                    Stream blobStream = blob.OpenReadAsync().Result;
                    return File(blobStream, blob.Properties.ContentType, blob.Name);
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("DownloadableForms", "WebsiteContent");
            }

        }

        // S C H O L A R S H I P S \\

        /*
         * Dispays the field for editing a selected scholarship, mainly the scholarship description.
         * */
        [HttpGet]
        public ActionResult UploadScholarshipDetails(int? scholarshipID)
        {
            try
            {
                if (scholarshipID == null)
                {
                    return RedirectToAction("ScholarshipSelection", "WebsiteContent");
                }
                else
                {
                    if (User.IsInRole("Administrator"))
                    {
                        return View(websiteActions.GetScholarshipByID((int)scholarshipID));
                    }
                    else
                    {
                        TempData["errorMessage"] = "Sorry you do not have access.";
                        return RedirectToAction("DownloadableForms", "WebsiteContent");
                    }                    
                }
            }
            catch (Exception e)
            {
                TempData["errorMessage"] = "There was an error in displaying the scholarship you wish to edit.";
                return RedirectToAction("WebsiteContent","ScholarshipSelection");
            }
        }

        /*
         * Updates the scholarship model to the database.
         * */
        [HttpPost]
        public ActionResult UploadScholarshipDetails(Scholarship scholarship, string newDescription)
        {
            try
            {
                scholarship.ScholarshipDescription = scholarship.ScholarshipDescription + " " + newDescription;
                ModelState.Remove("newDescription");
                if (ModelState.IsValid)
                {
                    if (User.IsInRole("Administrator"))
                    {
                        websiteActions.UpdateScholarship(scholarship);
                        return RedirectToAction("ScholarshipSelection", "WebsiteContent");
                    }
                    else
                    {
                        TempData["errorMessage"] = "Sorry you do not have access.";
                        return RedirectToAction("ScholarshipSelection", "WebsiteContent");
                    }
                }
                else
                {
                    return View(scholarship);
                }
            }
            catch (Exception e)
            {
                TempData["errorMessage"] = "There was an error in updating the scholarship you wish to edit." + e.ToString();
                return RedirectToAction("ScholarshipSelection");
            }
        }

    }
}