using F1Tipping.Common;
using F1Tipping.Data;
using F1Tipping.Data.AppModel;
using F1Tipping.PlayerData;
using F1Tipping.Tipping;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Platform;
using Quartz;

namespace F1Tipping;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = builder.Configuration;
        config.AddDetectedCommandLine(args);

        // Database setup, dependent on --provider cli arg
        // TODO: Move this to an extension and the new arg definition system.
        var provider = config.GetValue("provider", Provider.SqlServer.Name);
        void doDatabaseSetup(DbContextOptionsBuilder options)
        {
            if (provider == Provider.SqlServer.Name)
            {
                options.UseSqlServer(
                    config.GetConnectionString(Provider.SqlServer.Name)!,
                    x => x.MigrationsAssembly(Provider.SqlServer.Assembly)
                    );
            }
            else if (provider == Provider.Postgres.Name)
            {
                options.UseNpgsql(
                    config.GetConnectionString(Provider.Postgres.Name)!,
                    x => x.MigrationsAssembly(Provider.Postgres.Assembly)
                    ).UseLowerCaseNamingConvention();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        builder.Services.AddDbContext<AppDbContext>(doDatabaseSetup);
        builder.Services.AddDbContext<ModelDbContext>(doDatabaseSetup);

        builder.Services.AddScoped<DataSeeder>();
        builder.Services.AddScoped<CurrentDataService>();
        builder.Services.AddScoped<RoundOrchestrationService>();
        builder.Services.AddScoped<TipScoringService>();
        builder.Services.AddScoped<TipValidiationService>();
        builder.Services.AddScoped<PushNotificationsService>();
        builder.Services.AddScoped<NotificationScheduleService>();

        builder.Services.AddQuartz(q => q.RegisterCronJobs());

        builder.Services.AddHostedService<JobScheduleService>();
        builder.Services.AddHostedService<RoundOrchestrationServiceStarter>();
        builder.Services.AddHostedService<UserDataSeedingService>();
        builder.Services.AddHostedService<TippingDataSeedingService>();

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddDefaultIdentity<User>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
        }).AddRoles<IdentityRole<Guid>>()
          .AddEntityFrameworkStores<AppDbContext>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.MapControllers();
        app.MapRazorPages();
        app.UseAuthorization();

        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = (ctx) =>
            {
                ctx.Context.Response.Headers.Append("Service-Worker-Allowed", "/");
            }
        });

        app.MapRazorPages()
           .WithStaticAssets();

        app.UseForcePlayerInitialization();

        app.Run();
    }
}
