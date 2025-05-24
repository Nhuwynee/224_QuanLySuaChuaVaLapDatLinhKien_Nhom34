// using System.Diagnostics;
// using Microsoft.AspNetCore.Mvc;
// using QLSuaChuaVaLapDat.Models;

// namespace QLSuaChuaVaLapDat.Controllers
// {
//     public class HomeController : Controller
//     {
//         private readonly ILogger<HomeController> _logger;

//         public HomeController(ILogger<HomeController> logger)
//         {
//             _logger = logger;
//         }

//         public IActionResult Index()
//         {
//             return View();
//         }

//         public IActionResult Privacy()
//         {
//             return View();
//         }

//         [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//         public IActionResult Error()
//         {
//             return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//         }
//     }
// }


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using QLSuaChuaVaLapDat.Models;
using Microsoft.AspNetCore.Authorization;

public class HomeController : Controller
{
    private readonly QuanLySuaChuaVaLapDatContext _context;

    public HomeController(QuanLySuaChuaVaLapDatContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password, bool remember)
    {

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            //Truy vấn người dùng từ cơ sở dữ liệu
            var user = _context.Users.FirstOrDefault(u => u.TenUser == username && u.MatKhau == password);
            if (user != null)
            {

                // Lưu thông tin session
                HttpContext.Session.SetString("IdUser", user.IdUser);
                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("HoTen", user.HoVaTen);
                HttpContext.Session.SetString("Password", password); // không nên lưu password thật trong session

                // Chuyển hướng theo "loại người dùng" dựa trên username
                if (username.ToLower().Contains("cskh"))
                {
                    return RedirectToAction("IndexDSDK", "DanhSachDangKy");
                }

                else if (username.ToLower().Contains("admin"))
                {
                    return RedirectToAction();
                }
                else
                {
                    return RedirectToAction("Index", "Home"); // giao diện mặc định
                }
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return View("Index");
        }

        ViewBag.Error = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
        return View("Index");
    }

}
