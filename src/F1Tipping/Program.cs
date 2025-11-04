using F1Tipping.Data;
using F1Tipping.PlayerData;
using F1Tipping.Tipping;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using F1Tipping.Controllers;
using Lib.Net.Http.WebPush;

namespace F1Tipping;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = builder.Configuration;

        // Database setup, dependent on --provider cli arg
        var provider = config.GetValue("provider", Provider.SqlServer.Name);
        Action<DbContextOptionsBuilder> doDatabaseSetup = options =>
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
        };
        builder.Services.AddDbContext<AppDbContext>(doDatabaseSetup);
        builder.Services.AddDbContext<ModelDbContext>(doDatabaseSetup);

        builder.Services.AddScoped<DataSeeder>();
        builder.Services.AddScoped<TipScoringService>();
        builder.Services.AddScoped<TipValidiationService>();

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddDefaultIdentity<IdentityUser<Guid>>(options =>
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

        _ = Task.Run(async () =>
        {
            using (var scope = app.Services.CreateScope())
            {
                await SeedRolesAndUsers(scope);
                await SpamNotifications(scope, config);
            }
        });

        app.Run();
    }

    private static async Task SpamNotifications(IServiceScope scope, ConfigurationManager config)
    {
        var appDb = scope.ServiceProvider.GetService<AppDbContext>();

        var pushClient = new PushServiceClient();
        pushClient.DefaultAuthentication = new(
            config.GetValue<string>("Vapid:publicKey"),
            config.GetValue<string>("Vapid:privateKey"))
        {
            Subject = config.GetValue<string>("Vapid:subject")
        };

        while (true)
        {
            var subs = await appDb.GetPushSubscriptions(null);
            foreach (var sub in subs)
            {
                var pSub = new Lib.Net.Http.WebPush.PushSubscription();
                pSub.SetKey(PushEncryptionKeyName.P256DH, sub.PublicKey);
                pSub.SetKey(PushEncryptionKeyName.Auth, sub.AuthSecret);
                pSub.Endpoint = sub.DeviceEndpoint;

                var pMessage = new PushMessage("Mate. Get your tips in.");
                pMessage.Urgency = PushMessageUrgency.High;

                try
                {
                    await pushClient.RequestPushMessageDeliveryAsync(pSub, pMessage);
                }
                catch (Exception ex)
                {
                }
            }
            Thread.Sleep(60 * 1000);
        }
    }

    private static async Task SeedRolesAndUsers(IServiceScope scope)
    {
        string[] roles = [ "Administrator", "Player", ];
        (string,string)[] coreAdmins = [ ("admin@denholm.dev", "adminpass") ];

        var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetService<UserManager<IdentityUser<Guid>>>();

        foreach (var role in roles)
        {
            if (!await roleManager!.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                throwOnFailure(roleResult);
            }
        }

        foreach (var (email, pass) in coreAdmins)
        {
            var user = await userManager!.FindByEmailAsync(email);
            if (user is null)
            {
                var userResult = await userManager.CreateAsync(
                    new IdentityUser<Guid>(email)
                    {
                        Email = email,
                        EmailConfirmed = true,
                    }, pass);
                throwOnFailure(userResult);
                user = await userManager.FindByEmailAsync(email);
            }

            var userRoles = await userManager!.GetRolesAsync(user!);
            if (!userRoles.Contains("Administrator"))
            {
                var roleAssignment = await userManager.AddToRoleAsync(user!, "Administrator");
                throwOnFailure(roleAssignment);
            }
        }

        void throwOnFailure(IdentityResult? result)
        {
            if ((!result?.Succeeded) ?? true)
            {
                throw new ApplicationException($"Couldn't seed data! {result}");
            }
        }
    }
}
