using MCLScholarWeb.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MCLScholarWeb.Controllers
{
    public class RecordsController : Controller
    {
        // GET: Records
        private lipatdbEntities lipatdb;
        public RecordsController()
        {
            lipatdb = new lipatdbEntities();
        }
        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Students()
        {
            var student = lipatdb.UserProfiles.ToList();
            return View(student);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Requests()
        {
            var requests = lipatdb.ValidationRequests.ToList();
            return View(requests);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Scholars()
        {
            var requests = lipatdb.ValidationRequests.Where(e => e.ValidationStatu.StatusName.Equals("Approved")).ToList();
            return View();
        }
    }
}