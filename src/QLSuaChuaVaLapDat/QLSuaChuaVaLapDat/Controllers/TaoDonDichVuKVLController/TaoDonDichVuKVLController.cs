using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QLSuaChuaVaLapDat.Models;
using QLSuaChuaVaLapDat.Models.viewmodel;
using Newtonsoft.Json;

namespace QLSuaChuaVaLapDat.Controllers.TaoDonDichVuKVLController
{
    public class TaoDonDichVuKVLController : Controller
    {
        private readonly QuanLySuaChuaVaLapDatContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public TaoDonDichVuKVLController(QuanLySuaChuaVaLapDatContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }


        // Helper method to get the next service order ID
        private string GenerateNextServiceOrderId()
        {
            // Find the latest service order ID from the database
            var latestId = _context.DonDichVus
                .OrderByDescending(d => d.IdDonDichVu)
                .Select(d => d.IdDonDichVu)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(latestId))
            {
                // If no service order exists yet, start with "DDV001"
                return "DDV001";
            }

            // Extract the numeric part of the latest ID (e.g., "001" from "DDV001")
            if (latestId.Length > 3 && latestId.StartsWith("DDV"))
            {
                string numericPart = latestId.Substring(3);
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    // Increment the number and format it back with leading zeros
                    int nextNumber = lastNumber + 1;
                    return $"DDV{nextNumber:D3}"; // Format as DDV001, DDV002, etc.
                }
            }

            // Fallback if the ID format is unexpected
            return "DDV001";
        }

        // Phương thức tạo mã tự động cho nhiều loại ID
        private string GenerateNextId(string prefix, string latestId, int length)
        {
            if (string.IsNullOrEmpty(latestId) || !latestId.StartsWith(prefix))
            {
                return $"{prefix}{new string('0', length - prefix.Length)}1";
            }

            string numericPart = latestId.Substring(prefix.Length);
            if (int.TryParse(numericPart, out int lastNumber))
            {
                return $"{prefix}{(lastNumber + 1).ToString().PadLeft(length - prefix.Length, '0')}";
            }

            return $"{prefix}{new string('0', length - prefix.Length)}1";
        }

        // Tạo ID Chi Tiết Đơn Hàng
        //private string GenerateNextDetailId()
        //{
        //    var latestId = _context.ChiTietDonDichVus
        //        .OrderByDescending(d => d.IdCtdh)
        //        .Select(d => d.IdCtdh)
        //        .FirstOrDefault();

        //    return GenerateNextId("CT", latestId, 5);
        //}
        private string GenerateNextDetailId()
        {
            // Lấy ID gần nhất có tiền tố "CT"
            var latestId = _context.ChiTietDonDichVus
                .Where(d => d.IdCtdh.StartsWith("CT"))
                .OrderByDescending(d => d.IdCtdh)
                .Select(d => d.IdCtdh)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(latestId))
            {
                return "CT001";
            }

            string numericPart = latestId.Substring(2); // Bỏ "CT"
            if (int.TryParse(numericPart, out int lastNumber))
            {
                int nextNumber = lastNumber + 1;
                return $"CT{nextNumber:D3}"; // CT001, CT002, ...
            }

            return "CT001"; // Fallback nếu định dạng sai
        }


        // Tạo ID Hình Ảnh
        //private string GenerateNextImageId()
        //{
        //    var latestId = _context.HinhAnhs
        //        .OrderByDescending(ha => ha.IdHinhAnh)
        //        .Select(ha => ha.IdHinhAnh)
        //        .FirstOrDefault();

        //    return GenerateNextId("HA", latestId, 7); // HA00001, HA00002
        //}
        private string GenerateNextImageId()
        {
            var latestId = _context.HinhAnhs
                .Where(ha => ha.IdHinhAnh.StartsWith("HA"))
                .OrderByDescending(ha => ha.IdHinhAnh)
                .Select(ha => ha.IdHinhAnh)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(latestId))
            {
                return "HA00001";
            }

            string numericPart = latestId.Substring(2); // Bỏ "HA"
            if (int.TryParse(numericPart, out int lastNumber))
            {
                int nextNumber = lastNumber + 1;
                return $"HA{nextNumber:D5}"; // HA00001, HA00002, ...
            }

            return "HA00001"; // Fallback nếu định dạng sai
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

            //lấy danh sách lỗi
            var loaiLois = _context.LoaiLois
                .Select(ll => new SelectListItem
                {
                    Value = ll.IdLoi,
                    Text = ll.MoTaLoi
                }).ToList();

            ViewBag.LoaiLinhKiens = loaiLinhKiens;

            ViewBag.LoaiThietBis = loaiThietBis;

            ViewBag.LoaiLois = loaiLois;

            // Get the next service order ID
            ViewBag.NextServiceOrderId = GenerateNextServiceOrderId();

            ViewBag.OrderDate = DateTime.Now.ToString("dd/MM/yyyy");

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
        [HttpGet]
        public IActionResult LayGiaTheoLoi(string idLoi)
        {
            if (string.IsNullOrWhiteSpace(idLoi))
            {
                return Json(new { success = false, message = "Mã lỗi không hợp lệ" });
            }

            var giaLoi = _context.DonGia
                .Where(dg => dg.IdLoi == idLoi)
                .OrderByDescending(dg => dg.NgayCapNhat)
                .FirstOrDefault();

            if (giaLoi == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn giá cho lỗi này" });
            }

            return Json(new
            {
                success = true,
                gia = giaLoi.Gia,
                giaFormatted = String.Format("{0:n0}", giaLoi.Gia)
            });
        }

        // Lấy danh sách thành phố/tỉnh
        [HttpGet]
        public IActionResult LayDanhSachThanhPho()
        {
            var danhSachThanhPho = _context.ThanhPhos
                .Select(tp => new { id = tp.IdThanhPho, ten = tp.TenThanhPho })
                .ToList();

            return Json(danhSachThanhPho);
        }

        // Lấy danh sách quận/huyện theo thành phố
        [HttpGet]
        public IActionResult LayDanhSachQuan(string idThanhPho)
        {
            if (string.IsNullOrWhiteSpace(idThanhPho))
            {
                return Json(new { success = false, message = "Mã thành phố không hợp lệ" });
            }

            var danhSachQuan = _context.Quans
                .Where(q => q.IdThanhPho == idThanhPho)
                .Select(q => new { id = q.IdQuan, ten = q.TenQuan })
                .ToList();

            return Json(danhSachQuan);
        }

        // Lấy danh sách phường/xã theo quận
        [HttpGet]
        public IActionResult LayDanhSachPhuong(string idQuan)
        {
            if (string.IsNullOrWhiteSpace(idQuan))
            {
                return Json(new { success = false, message = "Mã quận không hợp lệ" });
            }

            var danhSachPhuong = _context.Phuongs
                .Where(p => p.IdQuan == idQuan)
                .Select(p => new { id = p.IdPhuong, ten = p.TenPhuong })
                .ToList();

            return Json(danhSachPhuong);
        }

        // API to get next service order ID
        [HttpGet]
        public IActionResult GetNextServiceOrderId()
        {
            string nextId = GenerateNextServiceOrderId();
            return Json(new { success = true, nextId = nextId });
        }

        // Handle form submission

        [HttpPost]
        public async Task<IActionResult> CreateServiceOrder([FromForm] ServiceOrderFormViewModel formData)
        {
            // Deserialize ErrorDetails nếu là string
            if ((formData.ErrorDetails == null || formData.ErrorDetails.Count == 0) && Request.Form.ContainsKey("ErrorDetails"))
            {
                var errorDetailsStr = Request.Form["ErrorDetails"];
                if (!string.IsNullOrEmpty(errorDetailsStr))
                {
                    formData.ErrorDetails = JsonConvert.DeserializeObject<List<ErrorDetailViewModel>>(errorDetailsStr);
                }
            }
            try
            {
                var userId = HttpContext.Session.GetString("IdUser");
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn hoặc chưa đăng nhập. Vui lòng đăng nhập lại!" });
                }

                // Bắt đầu transaction để đảm bảo tính nhất quán dữ liệu
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // 1. Tạo khách vàng lai
                        string idKhachVangLai = await TaoKhachVangLaiMoi(formData);

                        // 2. Tạo đơn dịch vụ
                        string idDonDichVu = await TaoDonDichVu(formData, idKhachVangLai);

                        // 3. Tạo chi tiết đơn và hình ảnh
                        await TaoChiTietDonVaHinhAnh(formData, idDonDichVu);

                        // Commit transaction nếu mọi thao tác đều thành công
                        transaction.Commit();

                        //return Json(new
                        //{
                        //    success = true,
                        //    message = "Đơn dịch vụ đã được tạo thành công!",
                        //    idDonDichVu = idDonDichVu
                        //});
                        // Trả về view IndexDSDDV với thông báo thành công

                        var danhSachDon = _context.DonDichVus.ToList();
                        return RedirectToAction("IndexDSDDV", "DanhSachDonDichVu");
                    }
                    catch (Exception ex)
                    {
                        // Rollback transaction nếu có lỗi
                        //transaction.Rollback();
                        //throw ex;
                        return Json(new { success = false, message = "Có lỗi xảy ra khi tạo đơn dịch vụ.", detailedMessage = ex.Message, innerException = ex.InnerException?.Message });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi tạo đơn dịch vụ: " + ex.ToString());
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // Tạo khách vãng lai
        private async Task<string> TaoKhachVangLaiMoi(ServiceOrderFormViewModel formData)
        {
            // Tạo idKhachVangLai mới (ví dụ: KVL001, KVL002,...)
            var last = _context.KhachVangLais.OrderByDescending(x => x.IdKhachVangLai).FirstOrDefault();
            string newId = "KVL001";

            if (last != null)
            {
                int num = int.Parse(last.IdKhachVangLai.Substring(3));
                newId = $"KVL{(num + 1):D3}";
            }

            // Lấy tên phường từ ID phường
            string tenPhuong = "";
            var phuong = await _context.Phuongs.FindAsync(formData.IdPhuong);
            if (phuong != null)
            {
                tenPhuong = phuong.TenPhuong;

                // Thêm thông tin quận/huyện và thành phố vào địa chỉ
                var quan = await _context.Quans.FindAsync(phuong.IdQuan);
                if (quan != null)
                {
                    tenPhuong += $", {quan.TenQuan}";

                    var thanhPho = await _context.ThanhPhos.FindAsync(quan.IdThanhPho);
                    if (thanhPho != null)
                    {
                        tenPhuong += $", {thanhPho.TenThanhPho}";
                    }
                }
            }

            // Ghép địa chỉ: Đường/Số nhà + ", " + Tên phường/xã + ", " + Tên quận/huyện + ", " + Tên thành phố
            string diaChi = formData.DuongSoNha?.Trim() ?? "";
            if (!string.IsNullOrEmpty(tenPhuong))
            {
                diaChi = string.IsNullOrEmpty(diaChi) ? tenPhuong : $"{diaChi}, {tenPhuong}";
            }

            // Tạo đối tượng KhachVangLai
            var khach = new KhachVangLai
            {
                IdKhachVangLai = newId,
                HoVaTen = formData.HoVaTen.Trim(),
                Sdt = formData.Sdt.Trim(),
                DiaChi = diaChi,
                IdPhuong = formData.IdPhuong
            };

            // Lưu vào database
            _context.KhachVangLais.Add(khach);
            await _context.SaveChangesAsync();

            return newId;
        }
        // Tạo đơn dịch vụ
        private async Task<string> TaoDonDichVu(ServiceOrderFormViewModel formData, string idKhachVangLai)
        {
            // Tạo ID đơn dịch vụ hoặc sử dụng ID đã có
            string idDonDichVu = string.IsNullOrEmpty(formData.IdDonDichVu) ?
                GenerateNextServiceOrderId() : formData.IdDonDichVu;

            // Xử lý trạng thái đơn
            string trangThai = formData.TrangThaiDon;
            if (trangThai == "Đã hoàn thành")
            {
                trangThai = "Hoàn thành";
            }

            // Tạo đối tượng DonDichVu
            var donDichVu = new DonDichVu
            {
                IdDonDichVu = idDonDichVu,
                IdUser = null, // Khách vãng lai không có IdUser
                IdKhachVangLai = idKhachVangLai,
                IdNhanVienKyThuat = formData.IdNhanVienKyThuat,
                IdUserTaoDon = HttpContext.Session.GetString("IdUser"), // Lấy ID người dùng từ session
                IdLoaiThietBi = formData.IdLoaiThietBi,
                TenThietBi = formData.TenThietBi,
                LoaiKhachHang = "Khách vãng lai",
                NgayTaoDon = DateTime.Now,
                NgayHoanThanh = formData.NgayHoanThanh,
                TongTien = formData.TongTien,
                HinhThucDichVu = formData.HinhThucDichVu,
                LoaiDonDichVu = formData.LoaiDonDichVu,
                PhuongThucThanhToan = null, // Sẽ được cập nhật khi thanh toán
                TrangThaiDon = trangThai ?? "Chưa hoàn thành",
                NgayChinhSua = DateTime.Now
            };

            // Lưu vào database
            _context.DonDichVus.Add(donDichVu);
            await _context.SaveChangesAsync();

            return idDonDichVu;
        }

        // Tạo chi tiết đơn dịch vụ và hình ảnh
        private async Task TaoChiTietDonVaHinhAnh(ServiceOrderFormViewModel formData, string idDonDichVu)
        {
            var donDichVu = await _context.DonDichVus.FindAsync(idDonDichVu);
            var loaiDichVu = formData.LoaiDichVu ?? donDichVu?.LoaiDonDichVu ?? "Sửa chữa";
            if (loaiDichVu != "Sửa chữa" && loaiDichVu != "Lắp đặt")
            {
                return;
            }
            // Xử lý từng chi tiết lỗi
            if (formData.ErrorDetails != null && formData.ErrorDetails.Count > 0)
            {
                foreach (var errorDetail in formData.ErrorDetails)
                {
                    await TaoMotChiTietDon(formData, idDonDichVu, errorDetail);
                }
            }

            //Xử lý các linh kiện được chọn riêng(nếu có)
            if (formData.SelectedPartIds != null && formData.SelectedPartIds.Count > 0)
            {
                //foreach (var idLinhKien in formData.SelectedPartIds)
                for (int i = 0; i < formData.SelectedPartIds.Count; i++)
                {
                    string idLinhKien = formData.SelectedPartIds[i];
                    string idLoi = null;
                    // Tạo chi tiết đơn hàng cho linh kiện được chọn riêng

                    // Lấy IdLoi tương ứng nếu có
                    if (formData.SelectedPartLoiIds != null && formData.SelectedPartLoiIds.Count > i)
                    {
                        idLoi = formData.SelectedPartLoiIds[i];
                    }
                    if (string.IsNullOrEmpty(idLoi))
                    {
                        // Gán mã lỗi mặc định hoặc trả về lỗi tùy nghiệp vụ
                        // idLoi = "LOI_MAC_DINH";
                        return; // hoặc throw exception nếu bắt buộc phải có lỗi
                    }
                    var linhKien = await _context.LinhKiens.FindAsync(idLinhKien);
                    //var linhKien = await _context.LinhKiens.FindAsync(idLinhKien);
                    string idCTDH = GenerateNextDetailId();

                    var chiTiet = new ChiTietDonDichVu
                    {
                        IdCtdh = idCTDH,
                        IdDonDichVu = idDonDichVu,
                        IdLinhKien = idLinhKien,
                        IdLoi = idLoi, // Không liên kết với lỗi nào cụ thể
                        LoaiDichVu = loaiDichVu,
                        MoTa = formData.MoTa,
                        SoLuong = 1,
                        ThoiGianThemLinhKien = DateTime.Now,
                        HanBaoHanh = false // Mặc định là hết bảo hành

                    };

                    // Lấy thông tin linh kiện để tính ngày kết thúc bảo hành
                    
                    if (linhKien != null)
                    {
                        var ngayHienTai = DateOnly.FromDateTime(DateTime.Now);
                        chiTiet.NgayKetThucBh = ngayHienTai.AddDays(linhKien.ThoiGianBaoHanh.DayNumber);
                    }

                    _context.ChiTietDonDichVus.Add(chiTiet);
                    await _context.SaveChangesAsync();
                }
            }
        }

        // Tạo một chi tiết đơn dịch vụ và các hình ảnh liên quan
        private async Task TaoMotChiTietDon(ServiceOrderFormViewModel formData, string idDonDichVu, ErrorDetailViewModel errorDetail)
        {
            var donDichVu = await _context.DonDichVus.FindAsync(idDonDichVu);
            var loaiDichVu = formData.LoaiDichVu ?? donDichVu?.LoaiDonDichVu ?? "Sửa chữa";
  
            if (loaiDichVu != "Sửa chữa" && loaiDichVu != "Lắp đặt")
            {
                return;
            }
            // Tạo ID chi tiết đơn hàng mới
            string idCTDH = GenerateNextDetailId();
            //var loaiDichVu = formData.LoaiDonDichVu?.Trim();
            //if (loaiDichVu != "Sửa chữa" && loaiDichVu != "Lắp đặt")
            //{
            //    // Có thể trả về lỗi hoặc gán mặc định
            //    loaiDichVu = "Sửa chữa"; // hoặc return lỗi
            //}
            // Tạo đối tượng chi tiết đơn dịch vụ
            var chiTiet = new ChiTietDonDichVu
            {
                IdCtdh = idCTDH,
                IdDonDichVu = idDonDichVu,
                IdLinhKien = errorDetail.IdLinhKien,
                IdLoi = errorDetail.IdLoi,
                LoaiDichVu = loaiDichVu,
                MoTa = errorDetail.MoTaLoi,
                SoLuong = errorDetail.SoLuong,
                ThoiGianThemLinhKien = DateTime.Now,
                HanBaoHanh = errorDetail.ConBaoHanh
            };

            // Tính ngày kết thúc bảo hành nếu có linh kiện thay thế
            if (!string.IsNullOrEmpty(errorDetail.IdLinhKien))
            {
                var linhKien = await _context.LinhKiens.FindAsync(errorDetail.IdLinhKien);
                if (linhKien != null)
                {
                    var ngayHienTai = DateOnly.FromDateTime(DateTime.Now);
                    chiTiet.NgayKetThucBh = ngayHienTai.AddDays(linhKien.ThoiGianBaoHanh.DayNumber);
                }
            }

            // Lưu chi tiết đơn hàng
            _context.ChiTietDonDichVus.Add(chiTiet);
            await _context.SaveChangesAsync();

            // Xử lý các hình ảnh
            await LuuHinhAnh(formData.DeviceImages, idCTDH, "thiết bị linh kiện");

            // Xử lý hình ảnh bảo hành (nếu còn bảo hành)
            if (errorDetail.ConBaoHanh)
            {
                await LuuHinhAnh(formData.WarrantyImages, idCTDH, "bảo hành");
            }
        }

        // Lưu hình ảnh vào thư mục và database
        private async Task LuuHinhAnh(List<IFormFile> images, string idCTDH, string loaiHinhAnh)
        {
            if (images == null || images.Count == 0)
                return;

            // Tạo thư mục lưu ảnh nếu chưa tồn tại
            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            foreach (var image in images)
            {
                if (image.Length <= 0)
                    continue;

                // Tạo tên file độc nhất
                string fileName = $"{Guid.NewGuid().ToString()}_{Path.GetFileName(image.FileName)}";
                string filePath = Path.Combine(uploadPath, fileName);

                // Lưu file vào thư mục
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Tạo ID hình ảnh mới
                string idHinhAnh = GenerateNextImageId();

                // Lưu thông tin vào database
                var hinhAnh = new HinhAnh
                {
                    IdHinhAnh = idHinhAnh,
                    IdCtdh = idCTDH,
                    Anh = $"/images/{fileName}",
                    LoaiHinhAnh = loaiHinhAnh
                };

                _context.HinhAnhs.Add(hinhAnh);
            }

            await _context.SaveChangesAsync();
        }
    }
}




