using MCLScholarWeb.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MCLScholarWeb.Actions
{
    public class WebSettings
    {

        public static ValidationPeriod CurrentPeriod()
        {
            lipatdbEntities db = new lipatdbEntities();
            return db.WebSettings.FirstOrDefault().ValidationPeriod;
        }

        public static string MaxDate()
        {
            lipatdbEntities db = new lipatdbEntities();
            DateTime date = (DateTime) db.ValidationPeriods.Max(model => model.Date);
            return date.ToString("yyyy-MM-dd");
        }

        public static string MinDate()
        {
            lipatdbEntities db = new lipatdbEntities();
            DateTime date = (DateTime) db.ValidationPeriods.Min(model => model.Date);
            return date.ToString("yyyy-MM-dd");
        }

        public static bool IsValidationOpen()
        {
            lipatdbEntities db = new lipatdbEntities();
            return db.WebSettings.FirstOrDefault().ValidationOpen;
        }
    }
}