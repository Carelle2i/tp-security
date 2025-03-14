using Duende.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AspNetCoreRateLimit;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using DotNetEnv;
using YourNamespace.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Charger les variables d'environnement depuis le fichier .env
Env.Load();

// Connexion à la base de données SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuration d'Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Politique de mots de passe
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 12;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false; // Aucun caractère spécial requis
    options.Password.RequiredUniqueChars = 6;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.ClaimsIdentity.UserIdClaimType = "sub";
    options.ClaimsIdentity.UserNameClaimType = "preferred_username";
});

// Récupérer le secret depuis les variables d'environnement (.env)
var secret = Environment.GetEnvironmentVariable("IDENTITY_SERVER_SECRET");
var hashedSecret = secret?.Sha256();

// Configuration de Duende IdentityServer
#pragma warning disable CS8604 // Possible null reference argument.
builder.Services.AddIdentityServer()
    .AddInMemoryClients(new[]
    {
        new Duende.IdentityServer.Models.Client
        {
            ClientId = "react-client",
            ClientSecrets = { new Duende.IdentityServer.Models.Secret(secret?.Sha256()) },
            AllowedGrantTypes = Duende.IdentityServer.Models.GrantTypes.Code,
            RedirectUris = { "http://localhost:3000/signin-oidc" },
            PostLogoutRedirectUris = { "http://localhost:3000" },
            AllowedScopes = { "openid", "profile", "api1" }
        }
    })
    .AddInMemoryApiScopes(new[]
    {
        new Duende.IdentityServer.Models.ApiScope("api1", "My API")
    })
    .AddDeveloperSigningCredential();
#pragma warning restore CS8604 // Possible null reference argument.

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configuration de Rate Limiting
builder.Services.AddInMemoryRateLimiting();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "10m",
            Limit = 100 // Limiter à 100 requêtes par période de 10 minutes
        }
    };
});
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
// builder.Services.AddSingleton<IRateLimitPolicyStore, MemoryCacheRateLimitPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Ajouter la configuration pour ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Ajout de l'IdentityServer et autres services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Ajouter les services de contrôleurs et API
builder.Services.AddControllers();

var app = builder.Build();

// Créer les rôles à l'initialisation
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
CreateRolesAndAdminUser(app.Services);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

// Utilisation du middleware pour le Rate Limiting
app.UseIpRateLimiting();

// Utilisation d'IdentityServer
app.UseIdentityServer();

app.MapControllers();

app.Run();

// Méthode pour créer les rôles et assigner un utilisateur Admin (si nécessaire)
async Task CreateRolesAndAdminUser(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    var roleNames = new[] { "Admin", "Utilisateur", "Invité" };

    // Créer les rôles s'ils n'existent pas
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Créer un utilisateur Admin par défaut si nécessaire
    var adminUser = await userManager.FindByEmailAsync("admin@example.com");
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin@example.com",
            Email = "admin@example.com",
            DateOfBirth = DateTime.Now.AddYears(-30) // Exemple de date de naissance
        };

        var createAdmin = await userManager.CreateAsync(adminUser, "Admin123@");
        if (createAdmin.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
