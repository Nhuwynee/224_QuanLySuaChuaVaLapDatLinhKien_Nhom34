using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSuaChuaVaLapDat.Models;

namespace QLSuaChuaVaLapDat.Controllers.EditTaoDonDichVuKVLController
{
    public class EditTaoDonDichVuKVLController : Controller
    {
        private readonly QuanLySuaChuaVaLapDatContext _context;
        public EditTaoDonDichVuKVLController(QuanLySuaChuaVaLapDatContext context)
        {
            _context = context;

        }
        public IActionResult IndexEditTaoDDVKVL(string id)
        {
            // Lưu id vào ViewBag để JavaScript có thể sử dụng nếu cần
            ViewBag.OrderId = id;

            // Lấy thông tin đơn từ database
            var donDichVu = _context.DonDichVus
                .FirstOrDefault(d => d.IdDonDichVu == id);

            if (donDichVu != null && donDichVu.NgayTaoDon.HasValue)
            {
                // Định dạng ngày tạo đơn thành dd/MM/yyyy
                ViewBag.OrderDate = donDichVu.NgayTaoDon.Value.ToString("dd/MM/yyyy");
            }
            else
            {
                ViewBag.OrderDate = DateTime.Now.ToString("dd/MM/yyyy");
            }

            if (donDichVu.IdNhanVienKyThuat != null && donDichVu.IdNhanVienKyThuatNavigation != null)
            {
                ViewBag.NhanVienKyThuatId = donDichVu.IdNhanVienKyThuat;
                ViewBag.StaffName = donDichVu.IdNhanVienKyThuatNavigation.HoVaTen;
                ViewBag.StaffSpecialty = donDichVu.IdNhanVienKyThuatNavigation.ChuyenMon;
            }

            var loaiThietBis = _context.ThietBis
                .Select(tb => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = tb.IdLoaiThietBi,
                    Text = tb.TenLoaiThietBi
                }).ToList();

            ViewBag.LoaiThietBis = loaiThietBis;

            // SỬAAA: Dữ liệu cho LoaiLinhKiens (đây phải là LOẠI linh kiện không phải linh kiện)
            var loaiLinhKiens = _context.LoaiLinhKiens  // Sửa: Truy vấn từ bảng LoaiLinhKien
                .Select(llk => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = llk.IdLoaiLinhKien,        // Sửa: Value là IdLoaiLinhKien
                    Text = llk.TenLoaiLinhKien         // Sửa: Text là TenLoaiLinhKien
                }).ToList();

            ViewBag.LoaiLinhKiens = loaiLinhKiens;     // Đặt tên ViewBag là LoaiLinhKiens

            // Dữ liệu cho LoaiLois
            var loaiLois = _context.LoaiLois
                .Select(l => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = l.IdLoi,
                    Text = l.MoTaLoi
                }).ToList();

            ViewBag.LoaiLois = loaiLois;

            return View();
        }

        [HttpGet]
        public IActionResult LayThongTinDon(string id)
        {
            try
            {
                // Sử dụng Include để tránh Null Reference Exception
                var donDichVu = _context.DonDichVus
                    .Include(d => d.IdUserNavigation)
                        .ThenInclude(u => u.IdPhuongNavigation)
                            .ThenInclude(p => p.IdQuanNavigation)
                                .ThenInclude(q => q.IdThanhPhoNavigation)
                    .Include(d => d.IdKhachVangLaiNavigation)
                        .ThenInclude(k => k.IdPhuongNavigation)
                            .ThenInclude(p => p.IdQuanNavigation)
                                .ThenInclude(q => q.IdThanhPhoNavigation)
                    .Include(d => d.ChiTietDonDichVus)
                        .ThenInclude(ct => ct.HinhAnhs)
                    .Include(d => d.IdNhanVienKyThuatNavigation) // Thêm dòng này để lấy thông tin nhân viên kỹ thuật
                    .FirstOrDefault(d => d.IdDonDichVu == id);

                if (donDichVu == null)
                    return NotFound();

                var result = new
                {
                    Don = new
                    {
                        donDichVu.IdDonDichVu,
                        donDichVu.IdUser,
                        donDichVu.IdKhachVangLai,
                        donDichVu.IdNhanVienKyThuat,
                        NhanVienKyThuat = donDichVu.IdNhanVienKyThuatNavigation != null ? new
                        {
                            IdNhanVien = donDichVu.IdNhanVienKyThuatNavigation.IdUser,
                            HoVaTen = donDichVu.IdNhanVienKyThuatNavigation.HoVaTen,
                            ChuyenMon = donDichVu.IdNhanVienKyThuatNavigation.ChuyenMon
                        } : null,
                        donDichVu.IdUserTaoDon,
                        donDichVu.IdLoaiThietBi,
                        donDichVu.TenThietBi,
                        donDichVu.LoaiKhachHang,
                        donDichVu.NgayTaoDon,
                        donDichVu.NgayHoanThanh,
                        donDichVu.TongTien,
                        //donDichVu.TienKhachHangUngTruoc,
                        donDichVu.HinhThucDichVu,
                        donDichVu.LoaiDonDichVu,
                        donDichVu.PhuongThucThanhToan,
                        donDichVu.TrangThaiDon,
                        donDichVu.NgayChinhSua
                    },
                    Khach = donDichVu.IdUserNavigation != null
                        ? new
                        {
                            IdUser = donDichVu.IdUserNavigation.IdUser,
                            HoVaTen = donDichVu.IdUserNavigation.HoVaTen,
                            Sdt = donDichVu.IdUserNavigation.Sdt,
                            // Email được bỏ qua theo yêu cầu
                            DuongSoNha = donDichVu.IdUserNavigation.DiaChi ?? "",
                            IdPhuong = donDichVu.IdUserNavigation.IdPhuong,
                            Phuong = donDichVu.IdUserNavigation?.IdPhuongNavigation?.TenPhuong ?? "",
                            IdQuan = donDichVu.IdUserNavigation?.IdPhuongNavigation?.IdQuanNavigation?.IdQuan,
                            Quan = donDichVu.IdUserNavigation?.IdPhuongNavigation?.IdQuanNavigation?.TenQuan ?? "",
                            IdThanhPho = donDichVu.IdUserNavigation?.IdPhuongNavigation?.IdQuanNavigation?.IdThanhPhoNavigation?.IdThanhPho,
                            ThanhPho = donDichVu.IdUserNavigation?.IdPhuongNavigation?.IdQuanNavigation?.IdThanhPhoNavigation?.TenThanhPho ?? ""
                        }
                        : donDichVu.IdKhachVangLaiNavigation != null
                            ? new
                            {
                                IdUser = donDichVu.IdKhachVangLaiNavigation.IdKhachVangLai,
                                HoVaTen = donDichVu.IdKhachVangLaiNavigation.HoVaTen,
                                Sdt = donDichVu.IdKhachVangLaiNavigation.Sdt,
                                // Email được bỏ qua theo yêu cầu
                                DuongSoNha = donDichVu.IdKhachVangLaiNavigation.DiaChi ?? "",
                                IdPhuong = donDichVu.IdKhachVangLaiNavigation.IdPhuong,
                                Phuong = donDichVu.IdKhachVangLaiNavigation?.IdPhuongNavigation?.TenPhuong ?? "",
                                IdQuan = donDichVu.IdKhachVangLaiNavigation?.IdPhuongNavigation?.IdQuanNavigation?.IdQuan,
                                Quan = donDichVu.IdKhachVangLaiNavigation?.IdPhuongNavigation?.IdQuanNavigation?.TenQuan ?? "",
                                IdThanhPho = donDichVu.IdKhachVangLaiNavigation?.IdPhuongNavigation?.IdQuanNavigation?.IdThanhPhoNavigation?.IdThanhPho,
                                ThanhPho = donDichVu.IdKhachVangLaiNavigation?.IdPhuongNavigation?.IdQuanNavigation?.IdThanhPhoNavigation?.TenThanhPho ?? ""
                            }
                            : null,
                    ChiTiet = donDichVu.ChiTietDonDichVus.Select(ct => new
                    {
                        ct.IdCtdh,
                        ct.IdDonDichVu,
                        ct.IdLinhKien,
                        ct.IdLoi,
                        ct.LoaiDichVu,
                        ct.MoTa,
                        ct.SoLuong,
                        ct.HanBaoHanh,
                        ct.NgayKetThucBh,
                        ct.ThoiGianThemLinhKien,
                        HinhAnh = ct.HinhAnhs.Select(h => h.Anh).ToList()
                    }).ToList(),
                    // Ảnh thiết bị và ảnh bảo hành
                    DeviceImages = donDichVu.ChiTietDonDichVus
                        .SelectMany(ct => ct.HinhAnhs.Where(h => h.LoaiHinhAnh == "thiết bị linh kiện").Select(h => h.Anh))
                        .ToList(),
                    WarrantyImages = donDichVu.ChiTietDonDichVus
                        .SelectMany(ct => ct.HinhAnhs.Where(h => h.LoaiHinhAnh == "Bảo hành").Select(h => h.Anh))
                        .ToList()
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error in LayThongTinDon: {ex.Message}");
                return StatusCode(500, new { error = "Đã xảy ra lỗi khi lấy thông tin đơn hàng" });
            }
        }

        [HttpGet]
        public IActionResult LayDanhSachNhanVienKyThuat(string chuyenMon = null)
        {
            try
            {
                // Query để lấy nhân viên kỹ thuật từ database
                var query = _context.Users.AsQueryable();

                // Nếu có chuyên môn, lọc theo chuyên môn
                if (!string.IsNullOrEmpty(chuyenMon))
                {
                    query = query.Where(nv => nv.ChuyenMon == chuyenMon);
                }

                // Lấy danh sách nhân viên
                var danhSachNhanVien = query
                    .Select(nv => new
                    {
                        nv.IdUser,
                        nv.HoVaTen,
                        nv.ChuyenMon,
                        //nv.TrangThai // Trạng thái có thể là "Sẵn sàng" hoặc "Đang bận"
                    })
                    .ToList();

                return Json(danhSachNhanVien);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Đã xảy ra lỗi khi lấy danh sách nhân viên kỹ thuật" });
            }
        }

        [HttpGet]
        public IActionResult LayThongTinLinhKien(string idLinhKien)
        {
            try
            {
                var linhKien = _context.LinhKiens
                    .Include(lk => lk.IdLoaiLinhKienNavigation) // Thêm Include để lấy thông tin loại linh kiện
                    .FirstOrDefault(lk => lk.IdLinhKien == idLinhKien);

                if (linhKien == null)
                    return NotFound();

                return Json(new
                {
                    idLinhKien = linhKien.IdLinhKien,
                    tenLinhKien = linhKien.TenLinhKien,
                    idLoaiLinhKien = linhKien.IdLoaiLinhKien,
                    tenLoaiLinhKien = linhKien.IdLoaiLinhKienNavigation?.TenLoaiLinhKien // Thêm tên loại linh kiện
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Đã xảy ra lỗi khi lấy thông tin linh kiện" });
            }
        }

        [HttpGet]
        public IActionResult LayLinhKienTheoLoai(string idLoaiLinhKien)
        {
            try
            {
                var linhKiens = _context.LinhKiens
                    .Where(lk => lk.IdLoaiLinhKien == idLoaiLinhKien)
                    .Select(lk => new
                    {
                        idLinhKien = lk.IdLinhKien,
                        tenLinhKien = lk.TenLinhKien,
                        idLoaiLinhKien = lk.IdLoaiLinhKien,
                        giaBan = lk.Gia
                    })
                    .ToList();

                return Json(linhKiens);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Đã xảy ra lỗi khi lấy danh sách linh kiện" });
            }
        }


        [HttpPost]
        public IActionResult UpdateServiceOrder(string IdDonDichVu, string PhuongThucThanhToan, string TrangThaiDon)
        {
            try
            {
                // Tìm đơn dịch vụ cần cập nhật
                var donDichVu = _context.DonDichVus.FirstOrDefault(d => d.IdDonDichVu == IdDonDichVu);

                if (donDichVu == null)
                {
                    return NotFound(new { error = "Không tìm thấy đơn dịch vụ" });
                }

                // Cập nhật phương thức thanh toán và trạng thái đơn
                if (!string.IsNullOrEmpty(PhuongThucThanhToan))
                {
                    donDichVu.PhuongThucThanhToan = PhuongThucThanhToan;
                }

                if (!string.IsNullOrEmpty(TrangThaiDon))
                {
                    donDichVu.TrangThaiDon = TrangThaiDon;
                }

                // Cập nhật ngày chỉnh sửa
                donDichVu.NgayChinhSua = DateTime.Now;

                // Lưu thay đổi vào database
                _context.SaveChanges();

                return Json(new { success = true, message = "Cập nhật đơn dịch vụ thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Đã xảy ra lỗi khi cập nhật đơn dịch vụ: " + ex.Message });
            }
        }
    }
}