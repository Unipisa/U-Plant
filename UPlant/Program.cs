using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Identity.Web;
using System.Globalization;
using System.Security.Claims;
using UPlant.Models;
using UPlant.Models.DB;
using UPlant.Models.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Identity.Web.UI;
using Microsoft.Extensions.FileProviders;
using Sustainsys.Saml2;
using Sustainsys.Saml2.AspNetCore2;
using Azure.Core;
using DocumentFormat.OpenXml.InkML;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Sustainsys.Saml2.Configuration;
using Azure;
using Microsoft.AspNetCore.Http.HttpResults;
using NuGet.Protocol;
using Microsoft.AspNetCore.Identity;

using System.Security.Cryptography.Xml;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.IdentityModel.Tokens.Saml2;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Net.Mail;
using System.Xml.Serialization;
using System.Text;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.IdentityModel.Tokens.Saml;
using System.Xml.Linq;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using NuGet.Versioning;

using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using NuGet.Protocol.Core.Types;

#region Services container



var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
ConfigurationManager StaticConfig = configuration;
IWebHostEnvironment env = builder.Environment;


var typeauth = configuration["AppSettings:Application:TypeAuth"];

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
//builder.Services.Configure<PathFile>(configuration.GetSection("Pathfiles"));

builder.Services.AddHttpContextAccessor();


//builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<Entities>(options => {
    options.UseSqlServer(configuration.GetConnectionString("UPlant"));
    options.EnableSensitiveDataLogging();

}
    );



builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("it")
    };
    options.DefaultRequestCulture = new RequestCulture(culture: "it", uiCulture: "it");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
builder.Services.AddTransient<IStringLocalizer, CustomLocalizer>();
#endregion
#region Autenticazione e Cookie
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    options.HandleSameSiteCookieCompatibility();
});
builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
   
    //options.AccessDeniedPath = new PathString(configuration.GetValue<string>("AccessDeniedPath"));
    options.AccessDeniedPath = new PathString("/Account/AccessDenied");
    options.LogoutPath = new PathString("/Account/SignOut");
    options.ReturnUrlParameter= new PathString("/Account/LoggedOut");
});
builder.Services.AddControllers().AddJsonOptions(x =>
   x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);
//controllo il tipo di autenticazione
if (typeauth == "AZURE")
{
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
      .AddMicrosoftIdentityWebApp(oAuthOptions =>
      {
      configuration.Bind("AzureAd", oAuthOptions);
      oAuthOptions.Events = new OpenIdConnectEvents();
      var onTokenValidated = oAuthOptions.Events.OnTokenValidated;

      oAuthOptions.Events.OnTokenValidated = context =>
      {
      onTokenValidated?.Invoke(context);
      var db = context.HttpContext.RequestServices.GetRequiredService<Entities>();

      var Utente = db.Users.Where(x => x.UnipiUserName == context.Principal.Identity.Name.Substring(0, context.Principal.Identity.Name.IndexOf("@"))); // unipiusername deve essere valorizzato solo con la parte iniziale dell'utente quindi es.a038858#unipi.it nel db è solo a038858 quindi devo fare la sub dell'identity.name
                                                                                                                                                       // Recupero tutti gli eventuali ruoli dell'utente



      var roles = (from ur in db.UserRole
                   join u in db.Users on ur.UserFK equals u.Id
                   join r in db.Roles on ur.RoleFK equals r.Id
                   where context.Principal.Identity.Name.StartsWith(u.UnipiUserName) && u.IsEnabled
                   select r.Descr
                        ).ToList();

      
      oAuthOptions.SaveTokens = true;
      


      if (roles.Count > 0)
      {
         
          var identity = (ClaimsIdentity)context.Principal.Identity;
          if (identity != null)
          {
              
              ((ClaimsIdentity)identity).AddClaims(
                      new[] {new Claim("principal", context.Principal.Identity.Name),
                          new Claim("sub", context.Principal.Identity.Name),
                      new Claim("UnipiUserID", context.Principal.Identity.Name),
                          new Claim("email", Utente.Select(x => x.Email).FirstOrDefault()),
                          new Claim("given_name", Utente.Select(x => x.Name).FirstOrDefault()),
                          new Claim("family_name", Utente.Select(x => x.LastName).FirstOrDefault()),
                          new Claim("fiscalNumber", Utente.Select(x => x.CF).FirstOrDefault())
                      });
                 
              }
              List<Claim> claims = new List<Claim>();
             
              foreach (var r in roles)
              {
                  claims.Add(new Claim(ClaimTypes.Role, r));

              }
              identity.AddClaims(claims);

           
          }
              return Task.CompletedTask;
          };
      }

  );


}



//qui metterò la configurazione WOS2

if (typeauth == "WSO2") { 
builder.Services.AddAuthentication(
    options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = configuration["WSo2Auth:Name"];

    })
    .AddCookie()



    .AddOAuth(configuration["WSo2Auth:Name"], oAuthOptions =>
    {
        oAuthOptions.ClientId = configuration["WSo2Auth:ClientID"];
        oAuthOptions.ClientSecret = configuration["WSo2Auth:ClientSecret"];
        oAuthOptions.AuthorizationEndpoint = configuration["WSo2Auth:AuthorizationEndpoint"];
        oAuthOptions.TokenEndpoint = configuration["WSo2Auth:TokenEndpoint"];
        oAuthOptions.UserInformationEndpoint = configuration["WSo2Auth:UserinfoEndpoint"];
        oAuthOptions.CallbackPath = new PathString(configuration["WSo2Auth:CallbackPath"]);
        oAuthOptions.Scope.Add(configuration["WSo2Auth:Scope"]);
        oAuthOptions.SaveTokens = true;

        foreach (string c in configuration.GetSection("WSo2Auth:Claims").Get<List<string>>())
        {
            oAuthOptions.ClaimActions.MapJsonKey(c, c);
        }

        // web can map json values to any standard-defined ClaimTypes names
        oAuthOptions.ClaimActions.MapJsonKey(ClaimTypes.UserData, "sub");
        // or use any name we prefer
        oAuthOptions.ClaimActions.MapJsonKey("UnipiUserID", "sub");

        var originalOnCreatingTicketEvent = oAuthOptions.Events.OnCreatingTicket;

        oAuthOptions.Events = new OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                oAuthOptions.ClaimActions.MapJsonKey(ClaimTypes.Name, "sub");
                originalOnCreatingTicketEvent?.Invoke(context);

                var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);

                string userinfo = await response.Content.ReadAsStringAsync();
                var myuser = JsonDocument.Parse(userinfo);

                context.RunClaimActions(myuser.RootElement);

                var Wso2user = System.Text.Json.JsonSerializer.Deserialize<UserFromWSo2>(userinfo);

                var db = context.HttpContext.RequestServices.GetRequiredService<Entities>();


                
                var roles = (from ur in db.UserRole
                             join u in db.Users on ur.UserFK equals u.Id
                             join r in db.Roles on ur.RoleFK equals r.Id
                             where context.Principal.Identity.Name.StartsWith(u.UnipiUserName) && u.IsEnabled
                             select r.Descr
                              ).ToList();

                if (roles.Count > 0)
                {
                    List<Claim> claims = new List<Claim>();

                    foreach (var r in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, r));
                    }
                    context.Identity.AddClaims(claims);
                }
                context.HttpContext.Response.Cookies.Append("unipiUplantAuthToken", context.AccessToken);
            }
        };
    });
}
//qui metterò la configurazione SAML2
if (typeauth == "SAML2")
{

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
        options.DefaultChallengeScheme = Saml2Defaults.Scheme;
        //options.DefaultChallengeScheme = configuration["WSo2Auth:Name"];

    })
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
   
    options.Events.OnValidatePrincipal = context =>
    {
        {
            // Grab the signature algorithm from the XML
            
            // Get ClaimsIdentity
            var identity = context.Principal.Identity as ClaimsIdentity;
            
            if (!identity.HasClaim("valorizzato","Yes")) { 


            string valcodfisetting = configuration["Saml2Auth:typeclaim_codicefiscale"];


            var valorecodicefiscale = identity.Claims.Where(x => x.Type == valcodfisetting).Select(x => x.Value).FirstOrDefault();
            var valoreuid = identity.Claims.Where(x => x.Type == configuration["Saml2Auth:typeclaim_udi"]).Select(x => x.Value).FirstOrDefault();
            identity.AddClaim(new Claim(ClaimTypes.Name, valoreuid));


            // Also put the somewhat hard to find Idp entity id into a claim by itself.






            var db = context.HttpContext.RequestServices.GetRequiredService<Entities>();



            var roles = (from ur in db.UserRole
                         join u in db.Users on ur.UserFK equals u.Id
                         join r in db.Roles on ur.RoleFK equals r.Id
                         where valorecodicefiscale == (u.CF) && u.IsEnabled
                         select r.Descr
                          ).ToList();


            var userSaml = db.Users.Where(x => x.CF == valorecodicefiscale).FirstOrDefault();

            List<Claim> claims = new List<Claim>();
            if (roles.Count > 0)
            {
                foreach (var r in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, r));
                }


            }
            claims.Add(new Claim("principal", valoreuid));
            claims.Add(new Claim("sub", valoreuid));
            claims.Add(new Claim("given_name", userSaml.Name));
            claims.Add(new Claim("family_name", userSaml.LastName));
            claims.Add(new Claim("email", userSaml.Email));
            claims.Add(new Claim("fiscalNumber", userSaml.CF));
            claims.Add(new Claim("valorizzato", "Yes"));


            identity.AddClaims(claims);
            //context.HttpContext.Response.Cookies.Append("unipiUplantAuthToken", context);
            context.HttpContext.Response.Cookies.Append("CustomCookie", "CustomValue", new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Imposta a true in produzione
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(30) // Scadenza del cookie
            });
        }
        }

        return Task.CompletedTask;
    };
   })

 
.AddSaml2(options =>
{
    options.SPOptions.EntityId = new Sustainsys.Saml2.Metadata.EntityId(configuration["Saml2Auth:EntityId"]);



    ServiceCertificate se = new ServiceCertificate()
    {
        Certificate = new X509Certificate2(configuration["Saml2Auth:SigningCertificateFile"], configuration["Saml2Auth:SigningCertificatePassword"]),
        Use = CertificateUse.Both,



    };
    // Configura il certificato (facoltativo se il metadata include il certificato)
    options.SPOptions.ServiceCertificates.Add(se);

   
   
    // Aggiungi il tuo IdP
    options.IdentityProviders.Add(
        new Sustainsys.Saml2.IdentityProvider(
            new Sustainsys.Saml2.Metadata.EntityId(configuration["Saml2Auth:IdpEntity"]), options.SPOptions)
        {
            LoadMetadata = true,
            AllowUnsolicitedAuthnResponse = true,
            
        });




});
}




    builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();
builder.Services.AddDirectoryBrowser();


#endregion
#region HTTP pipeline configuration
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
#endregion