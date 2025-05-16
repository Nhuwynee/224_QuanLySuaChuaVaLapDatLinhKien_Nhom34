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
        

        //lấy danh sách chuyên môn và nhân viên
        [HttpGet]
        public IActionResult LayDanhSachChuyenMon()
        {
            var chuyenMon = _context.Users
                .Where(u => u.ChuyenMon != null && u.ChuyenMon.Length > 0)
                .Select(u => u.ChuyenMon)
                .Distinct()
                .ToList();
                
            return Json(chuyenMon);
        }        
        
        [HttpGet]
        public IActionResult LayNhanVienTheoChuyenMon(string chuyenMon)
        {
            if (string.IsNullOrWhiteSpace(chuyenMon))
            {
                return Json(new List<UserViewModel>());
            }
            
            var nhanVien = _context.Users
                .Where(u => u.ChuyenMon == chuyenMon && u.IdRole == "R002")
                .Select(u => new UserViewModel
                {
                    IdUser = u.IdUser,
                    HoVaTen = u.HoVaTen,
                    ChuyenMon = u.ChuyenMon,
                    SDT = u.Sdt
                })
                .ToList();
                
            return Json(nhanVien);
        }


    }
}
