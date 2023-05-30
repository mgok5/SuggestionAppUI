using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Rewrite;
using SuggestionAppUI;



//var builder = WebApplication.CreateBuilder(args);

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    EnvironmentName = Environments.Staging
});

builder.ConfigureServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

//here added later
app.UseAuthentication();
app.UseAuthorization();

//here we modify our signout page if user sign out it redirect user to main page.
app.UseRewriter(new RewriteOptions().Add(
    context =>
    {
        if (context.HttpContext.Request.Path == "/MicrosoftIdentity/Account/SignedOut")
        {
            context.HttpContext.Response.Redirect("/");
        }
    }));

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
