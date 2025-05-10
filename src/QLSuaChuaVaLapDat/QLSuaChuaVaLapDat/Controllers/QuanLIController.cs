using Microsoft.AspNetCore.Mvc;
using QLSuaChuaVaLapDat.Models;
using System.Globalization;
using System.Linq;

namespace QLSuaChuaVaLapDat.Controllers
{

    public class QuanLIController : Controller
    {

        private readonly QuanLySuaChuaVaLapDatContext _context;

        public QuanLIController(QuanLySuaChuaVaLapDatContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var User = _context.User.ToList();
            return View(User); ;
        }
        public IActionResult thongKe()
        {
            var model = new ThongKeModel
            {
                TotalOrders = _context.DonDichVu.Count(),

                TotalRevenue = _context.DonDichVu.Sum(d => d.TongTien ?? 0),

                TotalStaff = _context.User.Count(u => u.IdRole == "R005"),

                CurrentMonth = DateTime.Now.ToString("MMMM yyyy", new CultureInfo("vi-VN"))
            };

            return View(model);
        }
    }
}
