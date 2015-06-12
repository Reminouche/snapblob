using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CarbonIT.Snapblob.Startup))]
namespace CarbonIT.Snapblob
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
