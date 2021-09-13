using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(KucykoweRodeo.Areas.Identity.IdentityHostingStartup))]
namespace KucykoweRodeo.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<Data.ArchiveContext>(options => options.UseSqlite(context.Configuration["ConnectionStrings:ArchiveContext"]));

                services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<Data.ArchiveContext>();
            });
        }
    }
}