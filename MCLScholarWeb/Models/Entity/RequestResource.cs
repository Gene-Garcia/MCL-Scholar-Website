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
    
    public partial class RequestResource
    {
        public int ResourceID { get; set; }
        public int FileID { get; set; }
        public int RequestID { get; set; }
    
        public virtual FileLocationStorage FileLocationStorage { get; set; }
        public virtual ValidationRequest ValidationRequest { get; set; }
    }
}