using Netia.Katharsys.Core.MVC;

namespace GeliboluIstihdam;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddDatabaseProfile
        (
            "Default",
            new Dictionary<string, string>()
            {
                { "Application Name", string.Format("NetiaKatharsysDataEngine-{0}", DatabaseMiddleware.DefaultConnectionAlias) }, //SELECT APP_NAME()
                { "Workstation ID", Environment.GetEnvironmentVariable("ComputerName") },
                { "Current Language", "Türkçe" },

                { "Data Source", @"cloud.gallikom.com" },
                //{ "Data Source", @"." },
                { "DataBase", "GeliboluIstihdam" },
                { "User ID", "sa" },
                { "Password", "Sql*admin1" },

                { "Persist Security Info", "False" },
                { "MultipleActiveResultSets", "True" }
            }
        );

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllerRoute
        (
            name: "AccountAction",
            pattern: "{area:exists}/{controller=Account}/{action=Index}"
        );

        app.MapControllerRoute(
            name: "Default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
