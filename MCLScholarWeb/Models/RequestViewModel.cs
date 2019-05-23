using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MCLScholarWeb.Models.Entity
{
    public class RequestViewModel
    {
        //todo: create factory class for this model
        public string AccountID { get; set; }
        public string Name { get; set; }
        public DateTime DateFilled { get; set; }
        public int StudentNumber { get; set; }
        public string TitleOfScholarship { get; set; }
        public string Email { get; set; }
        public string AcademicYear { get; set; }
        public int ApplictionNumber { get; set; }
        public string Remarks { get; set; }
        public List<string> Attatchments { get; set; }


    }
}