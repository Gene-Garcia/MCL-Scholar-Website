using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MCLScholarWeb.Models.Entity.Admin
{
    public class AdminService
    {
        lipatdbEntities db;
        public AdminService(lipatdbEntities db)
        {
            this.db = db;
        }

        public AdminEditViewModel GetViewModel(string id) {
            AspNetUser user = db.AspNetUsers.Where(uid => uid.Id.Equals(id)).FirstOrDefault();
            UserProfile profile = db.UserProfiles.Where(uid => uid.UserID.Equals(id)).FirstOrDefault();
            AspNetUserRole userRole = db.AspNetUserRoles.Where(role => role.UserId.Equals(id)).FirstOrDefault();
            AdminEditViewModel viewModel = new AdminEditViewModel();

            List<AspNetRole> roleList = db.AspNetRoles.ToList();
            List<string> roles = new List<string>();
            foreach (var role in roleList)
            {
                roles.Add(role.Name);
            }

            viewModel.EmailConfirmed = user.EmailConfirmed;
            viewModel.NameFirst = profile.StudentProfile.FirstName;
            viewModel.NameMiddle = profile.StudentProfile.MiddleName;
            viewModel.NameLast = profile.StudentProfile.LastName;
            viewModel.Program = user.UserPrograms.FirstOrDefault().Program.ProgramID;
            viewModel.StudentID = profile.StudentProfile.StudentID;
            viewModel.UserID = id;
            viewModel.Year = user.UserPrograms.FirstOrDefault().Year;
            viewModel.UserRole = userRole.AspNetRole.Name;
            viewModel.Roles = roles;
            return viewModel;
        }


    }
}