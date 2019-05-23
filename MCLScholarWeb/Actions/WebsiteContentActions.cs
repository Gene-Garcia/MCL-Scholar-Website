using MCLScholarWeb.Models.Entity;
using MCLScholarWeb.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

/*
 * Developer name: Gene Joseph Garcia
 * Description: This class will handle all the database manipulations of the content of the website content module.
 * 
 * */


namespace MCLScholarWeb.Actions
{
    public class WebsiteContentActions
    {
        private lipatdbEntities dbObject;
        private AzureBlobStorage storeAzureAnnouncements;
        private AzureBlobStorage storeAzureForms;

        public lipatdbEntities getDBObject() { return this.dbObject; }
        public AzureBlobStorage getAzureAnnouncementsObject() { return this.storeAzureAnnouncements; }
        public AzureBlobStorage getAzureFormsObject() { return this.storeAzureForms; }

        public WebsiteContentActions()
        {
            dbObject = new lipatdbEntities();
            storeAzureAnnouncements = new AzureBlobStorage(AzureBlobStorage.CONTAINER_ANNOUNCEMENT);
            storeAzureForms = new AzureBlobStorage(AzureBlobStorage.CONTAINER_FORMS);
        }

        // A N N O U N C E M E N T S \\
        
        public void UploadAnnouncement(Announcement announcementModel)
        {
            dbObject.Announcements.Add(announcementModel);
            dbObject.SaveChanges();
        }

        public List<Announcement> GetAnnouncements()
        {
            return dbObject.Announcements.Where(ann => ann.Status == "Active").OrderByDescending(model => model.DateAdded).ToList();

        }

        public Announcement GetAnnouncementByID(int announcementID)
        {
            return dbObject.Announcements.Where(announcementMod => announcementMod.AnnouncementID == announcementID && announcementMod.Status == "Active").ToList().FirstOrDefault();
        }

        public AnnouncementResource GetAnnouncementResourceByFileID(int fileID)
        {
            return dbObject.AnnouncementResources.Where(resource => resource.FileID == fileID).FirstOrDefault();
        }

        public void UpdateAnnouncement(Announcement announcementModel)
        {
            dbObject.Entry(announcementModel).State = System.Data.Entity.EntityState.Modified;
            dbObject.SaveChanges();
        }

        public string UploadAnnouncementBlobFile(HttpPostedFileBase photo)
        {
            bool isUploaded = storeAzureAnnouncements.UploadBlob(photo);

            string fileName = Path.GetFileName(photo.FileName);

            if (isUploaded)
            {
                return fileName;
            }
            else
            {
                return storeAzureAnnouncements.UploadBlobSimilarBlob(photo, dbObject.FileLocationStorages.Max(model => model.FileID));
            }
        }

        public void InsertPhotoToAnnouncementResources(Announcement announcementModel, BlobModel blobModel)
        {
            int newFileID = InsertBlobToFileLocationStorage(blobModel, "Announcement");

            AnnouncementResource announcementResource = new AnnouncementResource()
            {
                FileID = newFileID,
                AnnouncementID = announcementModel.AnnouncementID

            };

            dbObject.AnnouncementResources.Add(announcementResource);
            dbObject.SaveChanges();
        }

        public void DeleteFromAnnouncementResources(AnnouncementResource announcementResource)
        {
            dbObject.AnnouncementResources.Remove(announcementResource);
            dbObject.SaveChanges();
        }

        public bool DeleteAnnouncementFile(string fileName)
        {
            return storeAzureAnnouncements.DeleteBlob(fileName);
        }


        // D O W N L O A D A B L E  F O R M S \\

        public void UploadForm(Form formModel)
        {
            dbObject.Forms.Add(formModel);
            dbObject.SaveChanges();
        }

        public List<Form> GetAllForms()
        {
            return dbObject.Forms.OrderByDescending(model => model.DateAdded).ToList();
        }

        public Form GetFormByID(int formID)
        {
            return dbObject.Forms.Where(formModel => formModel.FormID == formID).FirstOrDefault();
        }

        public FormsResource GetFormResourceByFileID(int fileID)
        {
            return dbObject.FormsResources.Where(formResource => formResource.FileID == fileID).FirstOrDefault();
        }

        public void UpdateForm(Form formModel)
        {
            dbObject.Entry(formModel).State = System.Data.Entity.EntityState.Modified;
            dbObject.SaveChanges();
        }

        public void DeleteFromForms(Form form)
        {
            dbObject.Forms.Remove(form);
            dbObject.SaveChanges();
        }

        public string UploadFormBlobFile(HttpPostedFileBase file)
        {
            bool isUploaded = storeAzureForms.UploadBlob(file);

            string fileName = Path.GetFileName(file.FileName);

            if (isUploaded)
            {
                return fileName;
            }
            else
            {
                return storeAzureForms.UploadBlobSimilarBlob(file, dbObject.FileLocationStorages.Max(model => model.FileID));

            }
        }

        public bool DeleteFormFile(string fileName)
        {
            return storeAzureForms.DeleteBlob(fileName);
        }

        public void DeleteFromFileLocationStorage(FileLocationStorage fileLocationStorage)
        {
            dbObject.FileLocationStorages.Remove(fileLocationStorage);
            dbObject.SaveChanges();
        }

        public void InsertFormToFormResource(Form formModel, BlobModel blobModel)
        {
            int newFileID = InsertBlobToFileLocationStorage(blobModel, "Form");

            FormsResource formResource = new FormsResource()
            {
                FileID = newFileID,
                FormID = formModel.FormID
            };

            dbObject.FormsResources.Add(formResource);
            dbObject.SaveChanges();
        }

        public void DeleteFromFormResource(FormsResource formResource)
        {
            dbObject.FormsResources.Remove(formResource);
            dbObject.SaveChanges();
        }

        // S C H O L A R S H I P S \\

        public List<Scholarship> GetAllScholarshipType()
        {
            return dbObject.Scholarships.ToList();
        }
        internal List<Scholarship> GetAllScholarshipTypeThatAreListed()
        {
            return dbObject.Scholarships.Where(
                model => model.ScholarshipName.ToLower() != "sibling discount" &&
                model.ScholarshipName.ToLower() != "ygc promotional discount" &&
                model.ScholarshipName.ToLower() != "mcl study aid"
                ).ToList();
        }
        public Scholarship GetScholarshipByID(int scholarshipID)
        {
            return dbObject.Scholarships.Where(scholarship => scholarship.ScholarshipID == scholarshipID).FirstOrDefault();
        }
        public void UpdateScholarship(Scholarship scholarship)
        {
            dbObject.Entry(scholarship).State = System.Data.Entity.EntityState.Modified;
            dbObject.SaveChanges();
        }

        // S C H O L A R S \\

        public List<PreValidatedStudent> GetScholarsByScholarshipID(int scholarshipID)
        {
            return dbObject.PreValidatedStudents.Where(scholar => scholar.ScholarshipID == scholarshipID).ToList();
        }


        /***************************************************************************************/

        public FileLocationStorage GetFileLocationStorageByFileID(int fileID)
        {
            return dbObject.FileLocationStorages.Where(fileStorage => fileStorage.FileID == fileID).FirstOrDefault();
        }

        private int InsertBlobToFileLocationStorage(BlobModel blobModel, string type)
        {
            FileLocationStorage locStorage = new FileLocationStorage()
            {
                DateAdded = DateTime.Now.Date,
                FileLocation = blobModel.FileURI,
                FileName = HttpUtility.UrlDecode(blobModel.FileName),
                Type = type.ToUpper()
            };

            dbObject.FileLocationStorages.Add(locStorage);
            dbObject.SaveChanges();

            return locStorage.FileID;
        }

    }
}