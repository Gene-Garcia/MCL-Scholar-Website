using MCLScholarWeb.Models.Entity;
using MCLScholarWeb.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

/*
 * Developer name: Gene Joseph Garcia
 * Description: This class will handle all the database manipulations of the student functions.
 * 
 * */

namespace MCLScholarWeb.Actions
{
    public class StudentActions
    {
        private AzureBlobStorage azureBlobStorage;
        private lipatdbEntities dbObject;
        public StudentActions()
        {
            azureBlobStorage = new AzureBlobStorage(AzureBlobStorage.CONTAINER_REQUIREMENTS);
            dbObject = new lipatdbEntities();
        }

        public lipatdbEntities GetDBObject() { return this.dbObject; }

        public AzureBlobStorage GetAzureValidationReqObject()
        {
            return this.azureBlobStorage;
        }

        public IEnumerable<PreValidatedStudent> GetAllScholars()
        {
            return dbObject.PreValidatedStudents.ToList();
        }

        public Scholarship GetScholarshipByID(int scholarshipID)
        {
            return dbObject.Scholarships.Where(model => model.ScholarshipID == scholarshipID).FirstOrDefault();
        }

        public static bool IsAVoucher(Scholarship scholarship)
        {
            if (scholarship.ScholarshipType.ScholarshipTypeName.ToLower() == "voucher")
                return true;
            else
                return false;
        }

        public bool IsScholarshipRequested(int scholarshipID, string userID, int checkRequestType, int periodID)
        {
            ValidationRequest request = null;
            if (checkRequestType == 1) //check for voucher
            {
                request = dbObject.ValidationRequests.
                   Where(model => model.UserID == userID &&
                   model.Scholarship.ScholarshipType.ScholarshipTypeName.ToLower() == "voucher" &&
                   model.PeriodID == periodID
                   ).FirstOrDefault();
            }
            else if (checkRequestType == 2) //check for scholarship and financial aid
            {
                request = dbObject.ValidationRequests.
                    Where(model => model.UserID == userID &&
                    model.Scholarship.ScholarshipType.ScholarshipTypeName.ToLower() != "voucher" &&
                    model.PeriodID == periodID
                    ).FirstOrDefault();
            }

            return request != null ? true : false;
        }

        public List<ValidationRequest> GetMyRequest(int periodID, string userID)
        {
            return dbObject.ValidationRequests.Where(model => model.PeriodID == periodID && model.UserID == userID).OrderBy(model => model.DateFilled).ToList();
        }

        public ValidationRequest GetRequestByID(int requestID)
        {
            return dbObject.ValidationRequests.Where(model => model.RequestID == requestID).FirstOrDefault();
        }

        public void UploadStudentRequest(ValidationRequest requestModel)
        {
            dbObject.ValidationRequests.Add(requestModel);
            dbObject.SaveChanges();
        }

        public string UploadBlobFile(HttpPostedFileBase file)
        {
            bool isUploaded = azureBlobStorage.UploadBlob(file);

            string fileName = Path.GetFileName(file.FileName);

            if (isUploaded)
            {
                return fileName;
            }
            else
            {
                return azureBlobStorage.UploadBlobSimilarBlob(file, dbObject.FileLocationStorages.Max(model => model.FileID));
            }
        }

        public void UploadRequestToResource(ValidationRequest requestModel, BlobModel blobModel)
        {
            //insert to file location storage
            //get id of the file location storage
            int fileID = InsertBlobToFileLocationStorage(blobModel, "Requirements");

            //upload resource
            RequestResource resource = new RequestResource()
            {
                RequestID = requestModel.RequestID,
                FileID = fileID
            };

            dbObject.RequestResources.Add(resource);
            dbObject.SaveChanges();

        }

        public static bool IsScholarshipNonListed(string scholarshipName)
        {
            if (scholarshipName == "sibling discount" || scholarshipName == "ygc promotional discount" || scholarshipName == "mcl study aid")
                return true;
            return false;
        }

        private int InsertBlobToFileLocationStorage(BlobModel blobModel, string type)
        {
            FileLocationStorage locStorage = new FileLocationStorage()
            {
                DateAdded = DateTime.Now,
                FileLocation = blobModel.FileURI,
                FileName = blobModel.FileName,
                Type = type.ToUpper()
            };

            dbObject.FileLocationStorages.Add(locStorage);
            dbObject.SaveChanges();

            return locStorage.FileID;
        }

        public void UpdateStudentRequest(ValidationRequest requestModel)
        {
            dbObject.Entry(requestModel).State = System.Data.Entity.EntityState.Modified;
            dbObject.SaveChanges();
        }

        public RequestResource GetRequestResourceByFileID(int fileID)
        {
            return dbObject.RequestResources.Where(requestResource => requestResource.FileID == fileID).FirstOrDefault();
        }

        internal void DeleteFromRequestResource(RequestResource requestResource)
        {
            dbObject.RequestResources.Remove(requestResource);
            dbObject.SaveChanges();
        }

        public FileLocationStorage GetFileLocationStorageByFileID(int fileID)
        {
            return dbObject.FileLocationStorages.Where(fileStorage => fileStorage.FileID == fileID).FirstOrDefault();
        }

        public void DeleteFromFileLocationStorage(FileLocationStorage fileLocationStorage)
        {
            dbObject.FileLocationStorages.Remove(fileLocationStorage);
            dbObject.SaveChanges();
        }

        public bool IsStudentPassedApplicationForm(int scholarshipID, string userID)
        {
            ValidationRequest validationRequest = dbObject.ValidationRequests
                .Where(model => model.ScholarshipID == scholarshipID 
                && model.UserID == userID 
                && model.ValidationStatu.StatusName.ToLower() == "approved")
                .FirstOrDefault();
            
            return validationRequest != null ? true : false;
        }

        public bool DeleteRequestBlob(string fileName)
        {
            return azureBlobStorage.DeleteBlob(fileName);
        }

        public void InsertParent(int requestID, string firstNameParent, string middleNameParent, string lastNameParent)
        {

            SiblingDiscount siblingDiscount = new SiblingDiscount();

            Parent parent = dbObject.Parents.Where(model => model.FirstName.ToLower() == firstNameParent.ToLower() && model.MiddleName == middleNameParent.ToLower() && model.LastName == lastNameParent.ToLower()).FirstOrDefault();

            if (parent == null){
                //new parent
                Parent newParent = new Parent()
                {
                    FirstName = firstNameParent,
                    MiddleName = middleNameParent,
                    LastName = lastNameParent,
                };
                dbObject.Parents.Add(newParent);
                dbObject.SaveChanges();
            }

            siblingDiscount.RequestID = requestID;
            siblingDiscount.ParentID = parent.ParentID;
            dbObject.SiblingDiscounts.Add(siblingDiscount);
            dbObject.SaveChanges();
        }

        public string GetScholarshipName(int scholarshipID)
        {
            return dbObject.Scholarships.Where(model => model.ScholarshipID == scholarshipID).Select(model => model.ScholarshipName).FirstOrDefault();
        }


    }
}