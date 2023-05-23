using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace SuggestionAppUI;

public static class RegisterServices
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor().AddMicrosoftIdentityConsentHandler();
        builder.Services.AddMemoryCache(); // they also rely on the cache so we need this injection as well
        builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();



        //here we say we are using authentication system, use identity webapp authentication system and we passing builder.conf.getsec... in
        //and it is for authentication
        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdB2C"));



        // we do authorization here with this code. if B2C send info of jobtitle as admin then we'll give to the person admin authorization
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy =>
            {
                policy.RequireClaim("jobTitle", "Admin");
            });
        });



        builder.Services.AddSingleton<IDbConnection, DbConnection>();  // all below rely on IDbConnection 
        builder.Services.AddSingleton<ICategoryData, MongoCategoryData>();
        builder.Services.AddSingleton<IStatusData, MongoStatusData>();
        builder.Services.AddSingleton<ISuggestionData, MongoSuggestionData>();
        builder.Services.AddSingleton<IUserData, MongoUserData>();
    }
}
