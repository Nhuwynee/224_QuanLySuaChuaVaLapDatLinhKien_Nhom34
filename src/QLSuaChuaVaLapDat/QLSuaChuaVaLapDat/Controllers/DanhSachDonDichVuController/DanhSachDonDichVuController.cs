using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSuaChuaVaLapDat.Models;
using QLSuaChuaVaLapDat.Models.viewmodel;

namespace QLSuaChuaVaLapDat.Controllers.DanhSachDonDichVuController
{
    public class DanhSachDonDichVuController : Controller
    {
        private readonly QuanLySuaChuaVaLapDatContext _context;
        public DanhSachDonDichVuController(QuanLySuaChuaVaLapDatContext context)
        {
            _context = context;
        }
        //public IActionResult IndexDSDDV()
        //{
        //    var danhSach = (from don in _context.DonDichVus
        //        join ct in _context.ChiTietDonDichVus on don.IdDonDichVu equals ct.IdDonDichVu
        //        join u in _context.Users on don.IdUser equals u.IdUser into userJoin
        //        from u in userJoin.DefaultIfEmpty()
        //        join kvl in _context.KhachVangLais on don.IdKhachVangLai equals kvl.IdKhachVangLai into kvlJoin
        //        from kvl in kvlJoin.DefaultIfEmpty()
        //        group new { don, ct, u, kvl } by don.IdDonDichVu into g
        //        select new DonDichVuViewModel
        //        {
        //            MaDon = g.Key,
        //            TenKhachHang = g.FirstOrDefault().don.IdUser != null ?
        //                g.FirstOrDefault().u.HoVaTen :
        //                g.FirstOrDefault().kvl.HoVaTen,
        //            TrangThai = g.FirstOrDefault().don.TrangThaiDon,
        //            TongTien = g.FirstOrDefault().don.TongTien ?? 0,
        //            MoTa = string.Join(", ", g.Select(x => x.ct.MoTa))
        //        }).ToList();

        //    return View(danhSach);
        //}
        public IActionResult IndexDSDDV(int page = 1)
        {
            int pageSize = 5;

            var query = from don in _context.DonDichVus
                join ct in _context.ChiTietDonDichVus on don.IdDonDichVu equals ct.IdDonDichVu
                join u in _context.Users on don.IdUser equals u.IdUser into userJoin
                from u in userJoin.DefaultIfEmpty()
                join kvl in _context.KhachVangLais on don.IdKhachVangLai equals kvl.IdKhachVangLai into kvlJoin
                from kvl in kvlJoin.DefaultIfEmpty()
                group new { don, ct, u, kvl } by don.IdDonDichVu into g
                select new DonDichVuViewModel
                {
                    MaDon = g.Key,
                    TenKhachHang = g.FirstOrDefault().don.IdUser != null ?
                        g.FirstOrDefault().u.HoVaTen :
                        g.FirstOrDefault().kvl.HoVaTen,
                    TrangThai = g.FirstOrDefault().don.TrangThaiDon,
                    TongTien = g.FirstOrDefault().don.TongTien ?? 0,
                    MoTa = string.Join(", ", g.Select(x => x.ct.MoTa))
                };

            int totalCount = query.Count();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var danhSach = query
                .OrderBy(x => x.MaDon)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new DonDichVuPagedViewModel
            {
                DonDichVus = danhSach,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View("IndexDSDDV",viewModel);
        }

        [HttpGet]
        public IActionResult ChiTietDonDichVu(string id)
        {
            var chiTiet = (from don in _context.DonDichVus
                join tb in _context.ThietBis on don.IdLoaiThietBi equals tb.IdLoaiThietBi
                join nvkt in _context.Users on don.IdNhanVienKyThuat equals nvkt.IdUser
                join nguoiTao in _context.Users on don.IdUserTaoDon equals nguoiTao.IdUser
                join u in _context.Users on don.IdUser equals u.IdUser into userJoin
                from u in userJoin.DefaultIfEmpty()
                join kvl in _context.KhachVangLais on don.IdKhachVangLai equals kvl.IdKhachVangLai into kvlJoin
                from kvl in kvlJoin.DefaultIfEmpty()
                where don.IdDonDichVu == id
                select new ChiTietDonDichVuViewModel
                {
                    MaDon = don.IdDonDichVu,
                    TenKhachHang = don.IdUser != null ? u.HoVaTen : kvl.HoVaTen,
                    LoaiKhachHang = don.LoaiKhachHang,
                    NgayTaoDon = don.NgayTaoDon,
                    NgayHoanThanh = don.NgayHoanThanh,
                    TenNhanVienKyThuat = nvkt.HoVaTen,
                    TenNguoiTaoDon = nguoiTao.HoVaTen,
                    TenThietBi = don.TenThietBi,
                    LoaiThietBi = tb.TenLoaiThietBi,
                    HinhThucDichVu = don.HinhThucDichVu,
                    LoaiDonDichVu = don.LoaiDonDichVu,
                    PhuongThucThanhToan = don.PhuongThucThanhToan,
                    TongTien = don.TongTien ?? 0,
                    TrangThaiDon = don.TrangThaiDon,
                    NgayChinhSua = don.NgayChinhSua
                }).FirstOrDefault();

            if (chiTiet == null)
            {
                return NotFound();
            }

            return Json(chiTiet);
        }

        //chưa dám test
        [HttpPost]
        public IActionResult XoaDonDichVu(string id)
        {
            try
            {
                // Find the service order
                var donDichVu = _context.DonDichVus.Find(id);
                if (donDichVu == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn dịch vụ" });
                }

                // Find and delete related ChiTietDonDichVu records first
                var chiTietList = _context.ChiTietDonDichVus.Where(ct => ct.IdDonDichVu == id).ToList();

                foreach (var chiTiet in chiTietList)
                {
                    // Check if there are any HinhAnh records linked to this ChiTietDonDichVu
                    var hinhAnhList = _context.HinhAnhs.Where(h => h.IdCtdh == chiTiet.IdCtdh).ToList();
                    if (hinhAnhList.Any())
                    {
                        _context.HinhAnhs.RemoveRange(hinhAnhList);
                    }

                    _context.ChiTietDonDichVus.Remove(chiTiet);
                }

                // Check if there are any DanhGia records linked to this DonDichVu
                var danhGiaList = _context.DanhGia.Where(dg => dg.IdDonDichVu == id).ToList();
                if (danhGiaList.Any())
                {
                    _context.DanhGia.RemoveRange(danhGiaList);
                }

                // Remove the service order
                _context.DonDichVus.Remove(donDichVu);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                return Json(new { success = false, message = "Lỗi khi xóa đơn dịch vụ: " + ex.Message });
            }
        }

    }
}
