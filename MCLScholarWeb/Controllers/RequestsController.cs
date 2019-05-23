using MCLScholarWeb.Actions;
using MCLScholarWeb.Models.Entity;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MCLScholarWeb.Controllers
{
    public class RequestsController : Controller
    {
        // GET: Requests
        lipatdbEntities db;

        
        public RequestsController()
        {
            db = new lipatdbEntities();
        }

        // VALIDAION REQUESTS
        [Authorize(Roles = "Administrator,Co-Admin")]
        public ActionResult Index(string status = "Queue")
        {
            List<ValidationRequest> requests = null;
            ViewBag.Status = status;
            if (db.ValidationStatus.Where(e=>e.StatusName.Equals(status)).FirstOrDefault()==null)
            {
                ViewBag.Message = "Validation Status not found";
                return View();
            }
            int currentPeriodID = WebSettings.CurrentPeriod().PeriodID;
            requests = db.ValidationRequests.Where(e => e.ValidationStatu.StatusName.Equals(status) && e.PeriodID == currentPeriodID).ToList();
            return View(requests);
        }
        [HttpPost]
        public ActionResult Index(string status,string search) {
            List<ValidationRequest> requests = db.ValidationRequests.ToList();
            return View(requests);
        }
       

        [HttpPost]
        public ActionResult Index(int id,string status)
        {
            return RedirectToAction("View", new { id = id ,status=status});
        }

        [Authorize(Roles = "Administrator,Co-Admin")]
        public ActionResult View(int id,string status)
        {
            ViewBag.Status = status;
            ValidationRequest request = db.ValidationRequests.Where(e => e.RequestID.Equals(id)).FirstOrDefault();
            ViewBag.Statuses = db.ValidationStatus.ToList();

            return View(request);
        }

        [HttpPost]
        public ActionResult Submit(ValidationRequest request)
        {
            string id= User.Identity.GetUserId();
            //db.ValidationRequests.Where(e => e.RequestID.Equals(request.RequestID)).FirstOrDefault().AdminID = id;
            //db.ValidationRequests.Where(e => e.RequestID.Equals(request.RequestID)).FirstOrDefault().AspNetUser1 = db.AspNetUsers.Where(e => e.Id.Equals(id)).FirstOrDefault();
            //db.ValidationRequests.Where(e => e.RequestID.Equals(request.RequestID)).FirstOrDefault().DateEvaluated = DateTime.Now;
            //db.ValidationRequests.Where(e => e.RequestID.Equals(request.RequestID)).FirstOrDefault().StatusID = request.StatusID;
            //db.ValidationRequests.Where(e => e.RequestID.Equals(request.RequestID)).FirstOrDefault().Remarks = request.Remarks;

            request.AdminID = id;
            request.AspNetUser1 = db.AspNetUsers.Where(e => e.Id.Equals(id)).FirstOrDefault();
            request.DateEvaluated = DateTime.Now;
            request.ValidationStatu = db.ValidationStatus.Where(e => e.StatusID == request.RequestID).FirstOrDefault();

            db.Entry(request).State = System.Data.Entity.EntityState.Modified;

            db.SaveChanges();
            return RedirectToAction("Index");
        }
        // END OF VALIDATION REQUESTS

    }
}
