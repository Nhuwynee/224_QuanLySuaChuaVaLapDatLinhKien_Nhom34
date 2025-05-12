using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSuaChuaVaLapDat.Models;

namespace QLSuaChuaVaLapDat.Controllers.DanhSachDonDichVuController
{
    public class DanhSachDonDichVuController : Controller
    {
        private readonly QuanLySuaChuaVaLapDatContext _context;
        public DanhSachDonDichVuController(QuanLySuaChuaVaLapDatContext context)
        {
            _context = context;
        }
        public IActionResult IndexDSDDV()
        {
            return View();
        }
        

    }
}
