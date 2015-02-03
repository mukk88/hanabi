using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Hanabi.Startup))]
namespace Hanabi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            ConfigureAuth(app);
        }
    }
}
