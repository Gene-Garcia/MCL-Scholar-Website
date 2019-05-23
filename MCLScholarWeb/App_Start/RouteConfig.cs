using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MCLScholarWeb
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "WebsiteContent", action = "Announcement", id = UrlParameter.Optional },
                namespaces: new[] { "MCLScholarWeb.Controllers" }
            );

            //routes.MapRoute(
            //   name: "CreateNonStudent",
            //   url: "Account/CreateNonStudent/{role}",
            //   defaults: new { id = UrlParameter.Optional },
            //   namespaces: new[] { "MCLScholarWeb.Controllers" }
           //);

        }
    }
}
