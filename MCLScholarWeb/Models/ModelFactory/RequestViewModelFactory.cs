using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MCLScholarWeb.Models.Entity.ModelFactory
{
    public class RequestViewModelFactory
    {
        private lipatdbEntities entities;
        public RequestViewModelFactory(lipatdbEntities entities)
        {
            this.entities = entities;
        }
        public List<RequestViewModel> getList()
        {
            List<RequestViewModel> model = new List<RequestViewModel>();
            List<ValidationRequest> requests = entities.ValidationRequests.ToList();
           

            return model;
        }
    }


}