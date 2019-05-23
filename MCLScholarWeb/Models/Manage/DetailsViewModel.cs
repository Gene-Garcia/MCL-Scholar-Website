using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MCLScholarWeb.Models.Entity
{
    public class DetailsViewModel
    {
        public IndexViewModel IndexViewModel { get; set; }

        public UserProfile UserProfile { get; set; }

        public UserProgram UserProgram { get; set;  }


    }
}