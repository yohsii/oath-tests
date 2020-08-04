using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(oath_tests.Startup))]
namespace oath_tests
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
