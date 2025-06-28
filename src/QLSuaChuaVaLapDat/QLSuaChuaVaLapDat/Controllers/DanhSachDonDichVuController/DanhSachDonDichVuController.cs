using Microsoft.AspNetCore.Authorization;
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

            return View("IndexDSDDV", viewModel);
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
            // Lấy thông tin lỗi từ ChiTietDonDichVu
                var danhSachChiTietLoi = (from ctdv in _context.ChiTietDonDichVus
                join lk in _context.LinhKiens on ctdv.IdLinhKien equals lk.IdLinhKien into lkJoin
                from lk in lkJoin.DefaultIfEmpty()
                join ll in _context.LoaiLois on ctdv.IdLoi equals ll.IdLoi into llJoin
                from ll in llJoin.DefaultIfEmpty()
                where ctdv.IdDonDichVu == id
                select new ChiTietLoiViewModel { 
                    IdChiTiet = ctdv.IdCtdh,
                    TenLinhKien = lk != null ? lk.TenLinhKien : "Không có",                    
                    MoTaLoi = ll != null ? ll.MoTaLoi : "Không có",
                    MoTaChiTiet = ctdv.MoTa ?? "Không có",
                    // Ưu tiên lấy giá từ lỗi nếu có, nếu không thì lấy từ linh kiện
                    DonGiaLoi = (ll != null && ll.DonGia.Any()) 
                        ? ll.DonGia.OrderByDescending(dg => dg.NgayCapNhat).Select(dg => dg.Gia).FirstOrDefault()
                        : (lk != null ? lk.Gia : 0),
                    SoLuong = ctdv.SoLuong,
                    LoaiDichVu = ctdv.LoaiDichVu,
                    NgayKetThucBh = ctdv.NgayKetThucBh != null ? new DateTime(ctdv.NgayKetThucBh.Value.Year, ctdv.NgayKetThucBh.Value.Month, ctdv.NgayKetThucBh.Value.Day) : null,
                    ThoiGianThemLinhKien = ctdv.ThoiGianThemLinhKien,
                    HanBaoHanh = ctdv.HanBaoHanh,
                    // Thêm dòng sau để lấy danh sách ảnh
                    DanhSachAnh = _context.HinhAnhs
                        .Where(h => h.IdCtdh == ctdv.IdCtdh)
                        .Select(h => h.Anh) // hoặc h.TenFile nếu bạn lưu tên file
                        .ToList()
                }).ToList();

            if (danhSachChiTietLoi.Any())
            {
                chiTiet.DanhSachChiTietLoi = danhSachChiTietLoi;
                
                // Giữ lại cho tương thích với code cũ
                var chiTietDauTien = danhSachChiTietLoi.First();
                chiTiet.TenLinhKien = chiTietDauTien.TenLinhKien;
                chiTiet.MoTaLoi = chiTietDauTien.MoTaLoi;
                chiTiet.MoTaChiTiet = chiTietDauTien.MoTaChiTiet;
                chiTiet.DonGiaLoi = chiTietDauTien.DonGiaLoi;
            }
            else
            {
                chiTiet.TenLinhKien = "Không có";
                chiTiet.MoTaLoi = "Không có";
                chiTiet.MoTaChiTiet = "Không có";
                chiTiet.DonGiaLoi = 0;
            }

            return Json(chiTiet);
        }

        //chưa dám test
        [HttpPost]
        public IActionResult XoaDonDichVu(string id)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // Find the service order
                var donDichVu = _context.DonDichVus.Find(id);
                if (donDichVu == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn dịch vụ" });
                }

                // 1. Xóa hình ảnh trước (nếu có foreign key tới ChiTietDonDichVu)
                var chiTietList = _context.ChiTietDonDichVus.Where(ct => ct.IdDonDichVu == id).ToList();
                foreach (var chiTiet in chiTietList)
                {
                    var hinhAnhList = _context.HinhAnhs.Where(h => h.IdCtdh == chiTiet.IdCtdh).ToList();
                    if (hinhAnhList.Any())
                    {
                        _context.HinhAnhs.RemoveRange(hinhAnhList);
                    }
                }

                // 2. Xóa đánh giá (nếu có foreign key tới DonDichVu)
                var danhGiaList = _context.DanhGia.Where(dg => dg.IdDonDichVu == id).ToList();
                if (danhGiaList.Any())
                {
                    _context.DanhGia.RemoveRange(danhGiaList);
                }

                // 3. Xóa chi tiết đơn dịch vụ
                if (chiTietList.Any())
                {
                    _context.ChiTietDonDichVus.RemoveRange(chiTietList);
                }

                // 4. Cuối cùng mới xóa đơn dịch vụ
                _context.DonDichVus.Remove(donDichVu);

                // Save tất cả thay đổi
                _context.SaveChanges();

                // Commit transaction
                transaction.Commit();

                return Json(new { success = true, message = "Xóa đơn dịch vụ thành công" });
            }
            catch (Exception ex)
            {
                // Rollback nếu có lỗi
                transaction.Rollback();
                return Json(new { success = false, message = "Lỗi khi xóa đơn dịch vụ: " + ex.Message });
            }
        }

    }
}
