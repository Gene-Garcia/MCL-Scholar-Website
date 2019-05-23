using MCLScholarWeb.Models.Entity;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MCLScholarWeb.Actions
{
    public class ExcelActions
    {
        private lipatdbEntities db;
        public ExcelActions()
        {
            db = new lipatdbEntities(); ;
        }

        public bool IsExcelFileValid(ExcelPackage excel)
        {
            if (excel.Workbook.Worksheets.ToList().Count != 1)
            {
                return false;
            }

            ExcelWorksheet worksheet = excel.Workbook.Worksheets[1];
            for (int i = worksheet.Dimension.Start.Row + 11; i <= worksheet.Dimension.End.Row; i++)
            {
                string studentID = worksheet.Cells[i, 4].Value.ToString();
                AspNetUser user = db.AspNetUsers.Where(e => e.UserProfiles.First().StudentProfile.StudentID.Equals(studentID)).FirstOrDefault();
                if (user == null)
                {
                    return false;
                }
                string excelScholarshipType = worksheet.Cells[i, 5].Value.ToString();
                Scholarship type = db.Scholarships.Where(e => e.ScholarshipID.Equals(excelScholarshipType)).FirstOrDefault();
                if (type == null)
                {
                    return false;
                }
            }
            return true;
        }


        public ExcelPackage GenerateTemplate()
        {
            string templateLocation = HttpContext.Current.Server.MapPath("~/Content/Template.xlsx");
            ExcelPackage excel = new ExcelPackage(File.Open(templateLocation, FileMode.Open));
            ExcelWorksheet worksheet = excel.Workbook.Worksheets[2];
            int row = 3;
            int col = 1;
            //col 1 = Scholarship ID
            //col 2 = Scholarship Name
            foreach (var item in db.Scholarships)
            {
                worksheet.Cells[row, col].Value = item.ScholarshipID;
                worksheet.Cells[row, col+1].Value = item.ScholarshipName;
                row++;
            }
            return excel;
        }

    }
}





















