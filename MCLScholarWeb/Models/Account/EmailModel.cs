using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MCLScholarWeb.Models.Entity.Account
{
    public class EmailModel
    {
        [Display(Name = "Email address")]
        [Required(ErrorMessage = "The email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+(@(live.)?mcl.edu.ph)$", ErrorMessage = "Please Use your Live MCL Email Adress.")]
        public string Email { get; set; }
    }
}