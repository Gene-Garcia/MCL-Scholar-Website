//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MCLScholarWeb.Models.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class ValidationPeriod
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ValidationPeriod()
        {
            this.AspNetUsers = new HashSet<AspNetUser>();
            this.PreValidatedStudents = new HashSet<PreValidatedStudent>();
            this.Scholarships = new HashSet<Scholarship>();
            this.ValidationRequests = new HashSet<ValidationRequest>();
            this.WebSettings = new HashSet<WebSetting>();
        }
    
        public int PeriodID { get; set; }
        public int AcademicYearStart { get; set; }
        public int AcademicYearEnd { get; set; }
        public int Term { get; set; }
        public System.DateTime Date { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspNetUser> AspNetUsers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PreValidatedStudent> PreValidatedStudents { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Scholarship> Scholarships { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ValidationRequest> ValidationRequests { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WebSetting> WebSettings { get; set; }
    }
}