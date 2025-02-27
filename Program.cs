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
                { "Current Language", "Turkish" },

                { "Data Source", @"cloud.gallikom.com,14333\TEST" },
                //{ "Data Source", @"." },
                { "DataBase", "GeliboluIstihdam" },
                { "User ID", "sa" },
                { "Password", "Sql*admin1" },

                { "Persist Security Info", "False" },
                { "MultipleActiveResultSets", "True" }
            }
        );

        // Session yapılandırmasını ekle
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 dakika session timeout
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        // Session middleware'ini ekle (UseRouting'den sonra olmalı)
        app.UseSession();

        // Route yapılandırmasını düzeltelim
        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Account}/{action=Index}/{id?}");

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
