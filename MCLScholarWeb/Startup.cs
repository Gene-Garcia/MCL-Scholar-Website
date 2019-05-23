using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MCLScholarWeb.Startup))]
namespace MCLScholarWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }


    }
}
