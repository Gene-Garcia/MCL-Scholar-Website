using MCLScholarWeb.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Web;
using Microsoft.AspNet.Identity.Owin;

namespace MCLScholarWeb.Actions
{
    public class AuditActions
    {
        public static bool AddMessage(lipatdbEntities db,string message,string id) {
            if (message.Equals("")) {
                return false;
            }

            AspNetUser user = db.AspNetUsers.Where(e => e.Id.Equals(id)).FirstOrDefault();
            AdminAuditLog log = new AdminAuditLog()
            {
                DateTime = DateTime.Now,
                Event =message,
                UserID = user.Id,
            };
            db.AdminAuditLogs.Add(log);
            db.SaveChanges();
            return true;
        }
    }
}