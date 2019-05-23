using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MCLScholarWeb.Storage
{
    public class BlobModel
    {
        public string BlobContainerName { get; set; }
        public string FileURI { get; set; }
        public string FileName { get; set; }
    }
}