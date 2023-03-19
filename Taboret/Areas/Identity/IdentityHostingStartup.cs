using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Taboret.Areas.Identity;

[assembly: HostingStartup(typeof(IdentityHostingStartup))]
namespace Taboret.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                    .AddEntityFrameworkStores<Data.ArchiveContext>();
            });
        }
    }
}