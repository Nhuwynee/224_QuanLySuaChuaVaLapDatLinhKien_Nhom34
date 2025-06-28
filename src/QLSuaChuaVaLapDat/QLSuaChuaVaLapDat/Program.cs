
using Microsoft.EntityFrameworkCore;
using QLSuaChuaVaLapDat.Models;
using OfficeOpenXml;
using HIENMAUNHANDAO.BaoCao;
using PdfSharpCore.Fonts;
//ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<QuanLySuaChuaVaLapDatContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

GlobalFontSettings.FontResolver = new CustomFontResolver();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();

