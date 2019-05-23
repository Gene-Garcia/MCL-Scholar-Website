using System.IO;
using System.Linq;
using System.Web;

/*
 * Developer name: Gene Joseph Garcia
 * Description: This class will verify files if they are pdf or photos
 * 
 * */

namespace MCLScholarWeb.Actions
{
    public class FileVerification
    {

        //from website content actions
        public static string IsFileAPDF(HttpPostedFileBase file)
        {
            string allowedFile = ".pdf";
            string fileIsPDF = "";


            if (file != null)
            {
                if (Path.GetExtension(file.FileName).ToLower() == allowedFile)
                {
                    fileIsPDF = "pdf";
                }
                else
                {
                    fileIsPDF = "not";
                }
            }
            else
            {
                fileIsPDF = "null";
            }

            return fileIsPDF;
        }

        public static string AreFilesPhoto(HttpPostedFileBase[] photos)
        {
            string[] allowedFileName = { ".jpeg", ".png", ".jpg" };
            string fileIsAPhoto = "";

            if (photos != null && photos.Length > 0)
            {
                foreach (var photo in photos)
                {
                    if (photo != null)
                    {
                        if (allowedFileName.Contains(Path.GetExtension(photo.FileName).ToLower()))
                        {
                            fileIsAPhoto = "photo";
                        }
                        else
                        {
                            return "not";
                        }
                    }
                    else
                    {
                        return "null";
                    }
                }
            }
            return fileIsAPhoto;
        }

        public static bool AreFilesPDF(HttpPostedFileBase[] files)
        {
            string allowedFile = ".pdf";
            bool fileIsPdf = false;

            foreach (var file in files)
            {
                if (file != null)
                {
                    if (Path.GetExtension(file.FileName).ToLower() == allowedFile)
                    {
                        fileIsPdf = true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return fileIsPdf;
        }

    }
}