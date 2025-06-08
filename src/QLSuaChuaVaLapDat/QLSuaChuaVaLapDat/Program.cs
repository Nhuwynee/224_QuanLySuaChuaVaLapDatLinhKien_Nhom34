using HIENMAUNHANDAO.BaoCao;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PdfSharpCore.Fonts;
using QLSuaChuaVaLapDat.Models;
using System.ComponentModel;

ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddControllersWithViews();

GlobalFontSettings.FontResolver = new CustomFontResolver();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TimKiem}/{action=TimKiemDonDichVu}/{id?}");

app.Run();
