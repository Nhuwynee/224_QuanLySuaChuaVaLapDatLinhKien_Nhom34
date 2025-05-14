using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            //lấy danh sách thiết bị
            var loaiThietBis = _context.ThietBis
                .Select(tb => new SelectListItem
                {
                    Value = tb.IdLoaiThietBi,
                    Text = tb.TenLoaiThietBi
                }).ToList();

            //lấy danh sách loại linh kiện
            var loaiLinhKiens = _context.LoaiLinhKiens
                .Select(llk => new SelectListItem
                {
                    Value = llk.IdLoaiLinhKien,
                    Text = llk.TenLoaiLinhKien
                }).ToList();

            
            ViewBag.LoaiLinhKiens = loaiLinhKiens;

            ViewBag.LoaiThietBis = loaiThietBis;

            

            return View();
        }        
        
        [HttpGet]
        public IActionResult TimKiemLinhKien(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Json(new List<LinhKienViewModel>());
            }

            var result = (from lk in _context.LinhKiens
                join nsx in _context.NhaSanXuats on lk.IdNsx equals nsx.IdNsx
                where lk.TenLinhKien.ToLower().Contains(keyword.ToLower()) // Case insensitive search
                select new LinhKienViewModel
                {
                    Id = lk.IdLinhKien,
                    Ten = lk.TenLinhKien,
                    SoLuong = lk.SoLuong,
                    Gia = lk.Gia,
                    TenNSX = nsx.TenNsx
                }).Take(10).ToList(); // Limiting to 10 results for better performance

            return Json(result);
        }


    }
}
