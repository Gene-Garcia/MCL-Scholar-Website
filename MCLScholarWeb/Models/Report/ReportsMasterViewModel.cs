using MCLScholarWeb.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MCLScholarWeb.Models.Report
{
    public class ReportsMasterViewModel
    {
        public List<ValidationPeriod> ValidationPeriods { get; set; }
       public List<Scholarship> Scholarships { get; set; }
        public List<UserProfile> UserProfiles { get; set; }
        public List<ValidationRequest> ValidationRequests { get; set; }
    }
}