using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MCLScholarWeb.Models.Entity.Admin
{
    public class AdminEditViewModel
    {
        public string UserID { get; set; }
        public string UserRole { get; set; }
        public bool EmailConfirmed { get; set; }
        public string StudentID { get; set; }
        public string NameFirst { get; set; }
        public string NameMiddle { get; set; }
        public string NameLast { get; set; }
        public int Program { get; set; }
        public int Year { get; set; }

        public List<string> Roles { get; set; }




    }
}