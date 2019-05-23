using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DASHBOARD.Models
{
    public class DashboardModel
    {
        public string EarliestDateStart { get; set; }
        public string LatestDateEnd { get; set; }

        public string SelectedDateStart { get; set; }
        public string SelectedDateEnd { get; set; }

        public string YearsSelected { get; set; }

        public int ValidatedStudents { get; set; }
        public int NotValidatedStudents { get; set; }

        public int RegisteredStudents { get; set; }
        public int SHSStudents { get; set; }
        public int CollegeStudents { get; set; }

        public int AcademicScholars { get; set; }
        public int NonAcademicScholars { get; set; }

        public int Scholarships { get; set; }
        public List<string> ScholarshipNames { get; set; }
    }
}