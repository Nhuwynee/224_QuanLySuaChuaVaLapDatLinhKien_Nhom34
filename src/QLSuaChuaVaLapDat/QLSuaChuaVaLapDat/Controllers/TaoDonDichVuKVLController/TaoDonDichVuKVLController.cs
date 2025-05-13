using Microsoft.AspNetCore.Mvc;
using QLSuaChuaVaLapDat.Models;
using QLSuaChuaVaLapDat.Models.viewmodel;

namespace QLSuaChuaVaLapDat.Controllers.TaoDonDichVuKVLController
{
    public class TaoDonDichVuKVLController : Controller
    {
        private readonly QuanLySuaChuaVaLapDatContext _context;

        public TaoDonDichVuKVLController(QuanLySuaChuaVaLapDatContext context)
        {
            _context = context;
        }
        public IActionResult IndexTDDVKVL()
        {
            return View();
        }

        [HttpGet]
        public IActionResult TimKiemLinhKien(string keyword)
        {
            var result = (from lk in _context.LinhKiens
                join nsx in _context.NhaSanXuats on lk.IdNsx equals nsx.IdNsx
                where lk.TenLinhKien.Contains(keyword)
                select new LinhKienViewModel
                {
                    Id = lk.IdLinhKien,
                    Ten = lk.TenLinhKien,
                    SoLuong = lk.SoLuong,
                    Gia = lk.Gia,
                    TenNSX = nsx.TenNsx
                }).ToList();

            return Json(result);
        }


    }
}
