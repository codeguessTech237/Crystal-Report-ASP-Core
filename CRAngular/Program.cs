using Microsoft.AspNetCore.Http.Features;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";




builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });


// autoriser le telecharge de fichier de grande taille et de request longue
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue;
});

// PARAM-1 : autoriser des url different que l url en cours de faire de request http
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins,
                         policy =>
                         {
                             policy.WithOrigins("https://localhost:44466", "https://localhost:57915")
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                         });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}



app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Set the correct MIME type for JavaScript files
        if (ctx.File.Name.EndsWith(".js"))
        {
            ctx.Context.Response.Headers["Content-Type"] = "application/javascript";
        }
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.UseCors(MyAllowSpecificOrigins); // enregistrer des parametres  PARAM-1

app.Run();
