using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MCLScholarWeb.ViewModel
{
    public class AnnouncementViewModel
    {

        public int AnnouncementID { get; set; }
        public string AnnouncementSubject { get; set; } //missing in the announcement.cs
        public string AnnouncementText { get; set; }
        public System.DateTime DateAdded { get; set; }
        public string Status { get; set; }
        public List<string> ImageLocation  { get; set; }
        


    }
}