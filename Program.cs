using Aparteman.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using QuestPDF.Infrastructure;
//using static NPOI.XSSF.UserModel.Charts.XSSFLineChartData<Tx, Ty>;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Services --------------------

builder.Services
    .AddRazorPages()
    .AddRazorRuntimeCompilation();

builder.Services
    .AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new DataRowJsonConverter());
        options.SerializerSettings.StringEscapeHandling =
            Newtonsoft.Json.StringEscapeHandling.Default;
    });

// -------------------- App --------------------

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Other/500");
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseStatusCodePagesWithRedirects("/Other/{0}");

app.MapControllers();      // ⬅️ خیلی مهم
app.MapRazorPages();

app.Run();

/*

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorPages()
    .AddRazorRuntimeCompilation();

builder.Services
    .AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new DataRowJsonConverter());
        options.SerializerSettings.StringEscapeHandling =
            Newtonsoft.Json.StringEscapeHandling.Default;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Other/500");
}

static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            QuestPDF.Settings.License = LicenseType.Community;
        });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.UseStaticFiles();
app.UseStatusCodePagesWithRedirects("/Other/{0}");

app.Run();
*/