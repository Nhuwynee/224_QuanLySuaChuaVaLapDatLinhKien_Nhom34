using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QLSuaChuaVaLapDat.Models;
using QLSuaChuaVaLapDat.Models.viewmodel;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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

        //Tìm kiếm id khách
        [HttpGet]
        public IActionResult TimKiemKhachHang(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Json(new List<object>());

            var khachList = _context.Users
                .Where(kh => kh.IdRole == "R005" && // chỉ lấy khách có tài khoản
                             (kh.IdUser.Contains(keyword) || kh.HoVaTen.Contains(keyword)))
                .Select(kh => new
                {
                    id = kh.IdUser,
                    hoVaTen = kh.HoVaTen,
                    sdt = kh.Sdt,

                    duongSoNha = kh.DiaChi, // nếu có trường riêng thì sửa lại
                    idPhuong = kh.IdPhuong,
                    // Lấy tên phường, quận, thành phố
                    phuong = _context.Phuongs.Where(p => p.IdPhuong == kh.IdPhuong).Select(p => p.TenPhuong).FirstOrDefault(),
                    quan = (from p in _context.Phuongs
                            join q in _context.Quans on p.IdQuan equals q.IdQuan
                            where p.IdPhuong == kh.IdPhuong
                            select q.TenQuan).FirstOrDefault(),
                    thanhPho = (from p in _context.Phuongs
                                join q in _context.Quans on p.IdQuan equals q.IdQuan
                                join tp in _context.ThanhPhos on q.IdThanhPho equals tp.IdThanhPho
                                where p.IdPhuong == kh.IdPhuong
                                select tp.TenThanhPho).FirstOrDefault()
                })
                .Take(10)
                .ToList();

            return Json(khachList);
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


        [HttpGet]
        public IActionResult LayLinhKienTheoLoai(string idLoaiLinhKien)
        {
            if (string.IsNullOrWhiteSpace(idLoaiLinhKien))
            {
                return Json(new { success = false, message = "Mã loại linh kiện không hợp lệ" });
            }

            var linhKiens = _context.LinhKiens
                .Where(lk => lk.IdLoaiLinhKien == idLoaiLinhKien)
                .Join(_context.NhaSanXuats,
                    lk => lk.IdNsx,
                    nsx => nsx.IdNsx,
                    (lk, nsx) => new {
                        idLinhKien = lk.IdLinhKien,
                        tenLinhKien = lk.TenLinhKien,
                        gia = lk.Gia,
                        soLuong = lk.SoLuong,
                        tenNsx = nsx.TenNsx,
                        thoiGianBaoHanh = lk.ThoiGianBaoHanh
                    })
                .ToList();

            return Json(linhKiens);
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
            // Parse all form fields to ensure we get all data
            //foreach (var key in Request.Form.Keys)
            //{
            //    // id lỗi
            //    if (key.StartsWith("ErrorDetails[") && key.Contains("].IdLoi"))
            //    {
            //        var match = Regex.Match(key, @"ErrorDetails\[(\d+)\]");
            //        if (match.Success && int.TryParse(match.Groups[1].Value, out int index))
            //        {
            //            if (formData.ErrorDetails == null)
            //                formData.ErrorDetails = new List<ErrorDetailViewModel>();
            //            while (formData.ErrorDetails.Count <= index)
            //                formData.ErrorDetails.Add(new ErrorDetailViewModel());
            //            formData.ErrorDetails[index].IdLoi = Request.Form[key].ToString();
            //        }
            //    }

            //    //id linh kiện
            //    if (key.StartsWith("ErrorDetails[") && key.Contains("].IdLinhKien"))
            //    {
            //        // Extract the index from the key (e.g., "ErrorDetails[0].IdLinhKien")
            //        var match = Regex.Match(key, @"ErrorDetails\[(\d+)\]");
            //        if (match.Success && int.TryParse(match.Groups[1].Value, out int index))
            //        {
            //            // Ensure ErrorDetails list is initialized and has enough items
            //            if (formData.ErrorDetails == null)
            //            {
            //                formData.ErrorDetails = new List<ErrorDetailViewModel>();
            //            }

            //            while (formData.ErrorDetails.Count <= index)
            //            {
            //                formData.ErrorDetails.Add(new ErrorDetailViewModel());
            //            }

            //            // Set the IdLinhKien value
            //            formData.ErrorDetails[index].IdLinhKien = Request.Form[key].ToString();
            //        }
            //    }
            //}

            // Deserialize ErrorDetails nếu là string
            if ((formData.ErrorDetails == null || formData.ErrorDetails.Count == 0) && Request.Form.ContainsKey("ErrorDetails"))
            {
                var errorDetailsStr = Request.Form["ErrorDetails"];
                if (!string.IsNullOrEmpty(errorDetailsStr.ToString()))
                {
                    formData.ErrorDetails = JsonConvert.DeserializeObject<List<ErrorDetailViewModel>>(errorDetailsStr.ToString());
                    // Populate navigation properties for each error detail
                    if (formData.ErrorDetails != null)
                    {
                        foreach (var errorDetail in formData.ErrorDetails)
                        {
                            if (!string.IsNullOrEmpty(errorDetail.IdLoi))
                            {
                                errorDetail.Loi = await _context.LoaiLois.FindAsync(errorDetail.IdLoi);
                            }

                            if (!string.IsNullOrEmpty(errorDetail.IdLinhKien))
                            {
                                errorDetail.LinhKien = await _context.LinhKiens.FindAsync(errorDetail.IdLinhKien);
                            }
                        }
                    }
                }
            }
            // Get SelectedPartIds
            if ((formData.SelectedPartIds == null || formData.SelectedPartIds.Count == 0) && Request.Form.ContainsKey("SelectedPartIds"))
            {
                var partIdsStr = Request.Form["SelectedPartIds"];
                if (!string.IsNullOrEmpty(partIdsStr.ToString()))
                {
                    formData.SelectedPartIds = JsonConvert.DeserializeObject<List<string>>(partIdsStr.ToString());
                }
            }
            // Get SelectedPartLoiIds
            if ((formData.SelectedPartLoiIds == null || formData.SelectedPartLoiIds.Count == 0) && Request.Form.ContainsKey("SelectedPartLoiIds"))
            {
                var partLoiIdsStr = Request.Form["SelectedPartLoiIds"];
                if (!string.IsNullOrEmpty(partLoiIdsStr.ToString()))
                {
                    formData.SelectedPartLoiIds = JsonConvert.DeserializeObject<List<string>>(partLoiIdsStr.ToString());
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
                        //string idKhachVangLai = await TaoKhachVangLaiMoi(formData);
                        string idKhachVangLai = null;
                        // CHỈ tạo khách vãng lai nếu KHÔNG có IdUser
                        if (string.IsNullOrEmpty(formData.IdUser))
                        {
                            idKhachVangLai = await TaoKhachVangLaiMoi(formData);
                        }

                        // 2. Tạo đơn dịch vụ
                        string idDonDichVu = await TaoDonDichVu(formData, idKhachVangLai);

                        // 3. Tạo chi tiết đơn và hình ảnh
                        await TaoChiTietDonVaHinhAnh(formData, idDonDichVu);

                        // Commit transaction nếu mọi thao tác đều thành công
                        //await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        //return Json(new
                        //{
                        //    success = true,
                        //    message = "Đơn dịch vụ đã được tạo thành công!",
                        //    idDonDichVu = idDonDichVu
                        //});
                        // Trả về view IndexDSDDV với thông báo thành công

                        var danhSachDon = _context.DonDichVus.ToList();
                        //thông báo đã tạo thành công với popup alert

                        TempData["SuccessMessage"] = "Đơn dịch vụ đã được tạo thành công!";
                        return RedirectToAction("IndexDSDDV", "DanhSachDonDichVu");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo đơn dịch vụ: " + ex.Message;
                        return RedirectToAction("IndexDSDDV", "DanhSachDonDichVu");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi tạo đơn dịch vụ: " + ex.ToString());
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("IndexDSDDV", "DanhSachDonDichVu");
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

        // Phương thức hỗ trợ để lấy địa chỉ đầy đủ
        private async Task<string> GetDiaChiDayDu(string idPhuong)
        {
            string diaChi = "";
            var phuong = await _context.Phuongs.FindAsync(idPhuong);

            if (phuong != null)
            {
                diaChi = phuong.TenPhuong;

                var quan = await _context.Quans.FindAsync(phuong.IdQuan);
                if (quan != null)
                {
                    diaChi += $", {quan.TenQuan}";

                    var thanhPho = await _context.ThanhPhos.FindAsync(quan.IdThanhPho);
                    if (thanhPho != null)
                    {
                        diaChi += $", {thanhPho.TenThanhPho}";
                    }
                }
            }

            return diaChi;
        }

        // Phương thức hỗ trợ kết hợp địa chỉ
        private string CombineAddress(string duongSoNha, string diaChi)
        {
            duongSoNha = duongSoNha?.Trim() ?? "";
            return string.IsNullOrEmpty(duongSoNha) ? diaChi : $"{duongSoNha}, {diaChi}";
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
            // Convert total price from string if needed and ensure it's correctly formatted
            decimal tongTien = formData.TongTien;

            // If the amount seems suspiciously small (likely missing decimal places)
            if (tongTien > 0 && tongTien < 1000)
            {
                // Multiply by 1000 to fix common formatting issue
                tongTien *= 1000;
            }

            // Xác định khách hàng có tài khoản hay vãng lai
            string idUser = !string.IsNullOrEmpty(formData.IdUser) ? formData.IdUser : null;
            string idKhachVangLaiToSave = idUser == null ? idKhachVangLai : null;
            // Xác định loại khách hàng
            string loaiKhachHang = "Khách vãng lai";
            if (idKhachVangLaiToSave == null && idUser != null)
            {
                loaiKhachHang = "Khách hàng thường";
            }
            // Tạo đối tượng DonDichVu
            var donDichVu = new DonDichVu
            {
                IdDonDichVu = idDonDichVu,
                IdUser = idUser, // Nếu có mã khách hàng thì lưu vào đây
                IdKhachVangLai = idKhachVangLaiToSave, // Nếu không có mã khách thì lưu vào đây
                IdNhanVienKyThuat = formData.IdNhanVienKyThuat,
                IdUserTaoDon = HttpContext.Session.GetString("IdUser"), // Lấy ID người dùng từ session
                IdLoaiThietBi = formData.IdLoaiThietBi,
                TenThietBi = formData.TenThietBi,
                LoaiKhachHang = loaiKhachHang, // <-- Sửa dòng này,
                NgayTaoDon = DateTime.Now,
                NgayHoanThanh = formData.NgayHoanThanh,
                TongTien = tongTien,
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
                    if (string.IsNullOrEmpty(idLinhKien))
                    {
                        // Gán mã lỗi mặc định hoặc trả về lỗi tùy nghiệp vụ
                        // idLoi = "LOI_MAC_DINH";
                        continue; // hoặc throw exception nếu bắt buộc phải có lỗi
                    }
                    var linhKien = await _context.LinhKiens.FindAsync(idLinhKien);
                    if (linhKien == null)
                    {
                        continue; // Bỏ qua nếu linh kiện không tồn tại
                    }
                    //var linhKien = await _context.LinhKiens.FindAsync(idLinhKien);
                    string idCTDH = GenerateNextDetailId();

                    // Calculate warranty end date if applicable (for selected parts without error details)
                    DateOnly? warrantyEndDate = null;

                    // Default warranty period for separate parts (can be modified as needed)
                    int warrantyPeriodMonths = linhKien.ThoiGianBaoHanh;

                    // Only calculate if order is completed and we have warranty information
                    if (

                        donDichVu.NgayHoanThanh.HasValue &&
                        warrantyPeriodMonths > 0)
                    {
                        // Get completion date
                        DateTime completionDate = donDichVu.NgayHoanThanh.Value;

                        // Add warranty months to completion date
                        DateTime endWarrantyDate = completionDate.AddMonths(warrantyPeriodMonths);

                        // Convert to DateOnly format
                        warrantyEndDate = DateOnly.FromDateTime(endWarrantyDate);
                    }

                    var chiTiet = new ChiTietDonDichVu
                    {
                        IdCtdh = idCTDH,
                        IdDonDichVu = idDonDichVu,
                        IdLinhKien = idLinhKien,
                        IdLoi = idLoi,
                        LoaiDichVu = loaiDichVu,
                        MoTa = formData.MoTa,
                        SoLuong = 1,
                        ThoiGianThemLinhKien = DateTime.Now,
                        NgayKetThucBh = warrantyEndDate, // Ngày kết thúc bảo hành từ form
                        HanBaoHanh = false // Mặc định là hết bảo hành

                    };

                    // Lấy thông tin linh kiện để tính ngày kết thúc bảo hành

                    //var ngayHienTai = DateOnly.FromDateTime(DateTime.Now);
                    //if (linhKien.ThoiGianBaoHanh != null)
                    //{
                    //    // Sử dụng trực tiếp giá trị ThoiGianBaoHanh làm ngày kết thúc bảo hành
                    //    chiTiet.NgayKetThucBh = linhKien.ThoiGianBaoHanh;
                    //}

                    _context.ChiTietDonDichVus.Add(chiTiet);
                    await _context.SaveChangesAsync();
                }
            }
        }

        // Tạo một chi tiết đơn dịch vụ và các hình ảnh liên quan
        // Update the TaoMotChiTietDon method

        private async Task TaoMotChiTietDon(ServiceOrderFormViewModel formData, string idDonDichVu, ErrorDetailViewModel errorDetail)
        {
            var donDichVu = await _context.DonDichVus.FindAsync(idDonDichVu);
            var loaiDichVu = formData.LoaiDichVu ?? donDichVu?.LoaiDonDichVu ?? "Sửa chữa";

            if (loaiDichVu != "Sửa chữa" && loaiDichVu != "Lắp đặt")
            {
                return;
            }

            // Direct connection to database to verify IDs exist before assignment
            string validIdLinhKien = null;
            string validIdLoi = null;
            int warrantyPeriodMonths = 0;
            // Validate IdLinhKien by directly checking the database
            if (!string.IsNullOrEmpty(errorDetail.IdLinhKien))
            {
                var linhKien = await _context.LinhKiens.FirstOrDefaultAsync(lk => lk.IdLinhKien == errorDetail.IdLinhKien.Trim());
                if (linhKien != null)
                {
                    validIdLinhKien = linhKien.IdLinhKien;
                    errorDetail.LinhKien = linhKien;
                    // Get warranty period from the selected part
                    warrantyPeriodMonths = linhKien.ThoiGianBaoHanh;
                }
                else
                {
                    Console.WriteLine($"Warning: LinhKien ID not found: {errorDetail.IdLinhKien}");
                }
            }

            // Validate IdLoi by directly checking the database
            if (!string.IsNullOrEmpty(errorDetail.IdLoi))
            {
                var loi = await _context.LoaiLois.FirstOrDefaultAsync(l => l.IdLoi == errorDetail.IdLoi.Trim());
                if (loi != null)
                {
                    validIdLoi = loi.IdLoi;
                    errorDetail.Loi = loi;
                }
                else
                {
                    Console.WriteLine($"Warning: Loi ID not found: {errorDetail.IdLoi}");
                }
            }

            // Skip creating this record if both IDs are invalid
            //if (string.IsNullOrEmpty(validIdLinhKien) && string.IsNullOrEmpty(validIdLoi))
            //{
            //    Console.WriteLine("Warning: Both IdLinhKien and IdLoi are invalid. Skipping this record.");
            //    return;
            //}

            // Tạo ID chi tiết đơn hàng mới
            string idCTDH = GenerateNextDetailId();

            // Calculate warranty end date if applicable
            DateOnly? warrantyEndDate = null;

            // Only calculate warranty end date if:
            // 1. The order status is "Hoàn thành" or "Đã hoàn thành"
            // 2. The warranty flag is true
            // 3. We have valid completion date and warranty period
            if (
                //errorDetail.ConBaoHanh == false &&
                donDichVu.NgayHoanThanh.HasValue &&
                warrantyPeriodMonths > 0)
            {
                // Get completion date
                DateTime completionDate = donDichVu.NgayHoanThanh.Value;

                // Add warranty months to completion date
                DateTime endWarrantyDate = completionDate.AddMonths(warrantyPeriodMonths);

                // Convert to DateOnly format
                warrantyEndDate = DateOnly.FromDateTime(endWarrantyDate);
            }
            // If explicitly marked as under warranty but no end date specified, 
            // use the warranty period from the part if available
            else if (errorDetail.ConBaoHanh && validIdLinhKien != null &&
                     donDichVu.NgayHoanThanh.HasValue && warrantyPeriodMonths > 0)
            {
                // Calculate warranty end date from completion date
                DateTime completionDate = donDichVu.NgayHoanThanh.Value;
                DateTime endWarrantyDate = completionDate.AddMonths(warrantyPeriodMonths);
                warrantyEndDate = DateOnly.FromDateTime(endWarrantyDate);
            }
            // If user provided a specific warranty end date, use that
            else if (errorDetail.NgayKetThucBaoHanh.HasValue)
            {
                warrantyEndDate = errorDetail.NgayKetThucBaoHanh;
            }

            // Create the ChiTietDonDichVu with validated IDs
            var chiTiet = new ChiTietDonDichVu
            {
                IdCtdh = idCTDH,
                IdDonDichVu = idDonDichVu,
                IdLinhKien = validIdLinhKien, // Use validated ID
                IdLoi = validIdLoi, // Use validated ID
                LoaiDichVu = loaiDichVu,
                MoTa = errorDetail.MoTaLoi,
                SoLuong = errorDetail.SoLuong,
                ThoiGianThemLinhKien = DateTime.Now,
                NgayKetThucBh = warrantyEndDate, // Ngày kết thúc bảo hành từ form
                HanBaoHanh = errorDetail.ConBaoHanh
            };

            // Calculate warranty end date if we have a valid linh kien
            //if (!string.IsNullOrEmpty(validIdLinhKien) && errorDetail.LinhKien != null)
            //{
            //    // Sử dụng trực tiếp giá trị ThoiGianBaoHanh từ linh kiện
            //    chiTiet.NgayKetThucBh = errorDetail.LinhKien.ThoiGianBaoHanh;
            //}
            //else if (errorDetail.NgayKetThucBaoHanh.HasValue)
            //{
            //    chiTiet.NgayKetThucBh = errorDetail.NgayKetThucBaoHanh;
            //}

            // Add to database and save
            _context.ChiTietDonDichVus.Add(chiTiet);
            try
            {
                await _context.SaveChangesAsync();

                // Process images only after successfully saving the record
                await LuuHinhAnh(formData.DeviceImages, idCTDH, "thiết bị linh kiện");

                // Process warranty images if applicable
                if (errorDetail.ConBaoHanh)
                {
                    await LuuHinhAnh(formData.WarrantyImages, idCTDH, "bảo hành");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving ChiTietDonDichVu: {ex.Message}");
                throw; // Re-throw to be caught by the transaction
            }
        }

        // Lưu hình ảnh vào thư mục và database
        private async Task LuuHinhAnh(List<IFormFile> images, string idCTDH, string loaiHinhAnh)
        {
            if (images == null || images.Count == 0)
                return;

            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Lấy ID hình ảnh lớn nhất hiện tại
            var latestId = _context.HinhAnhs
                .Where(ha => ha.IdHinhAnh.StartsWith("HA"))
                .OrderByDescending(ha => ha.IdHinhAnh)
                .Select(ha => ha.IdHinhAnh)
                .FirstOrDefault();

            int baseNumber = 1;
            if (!string.IsNullOrEmpty(latestId))
            {
                string numericPart = latestId.Substring(2);
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    baseNumber = lastNumber + 1;
                }
            }

            for (int i = 0; i < images.Count; i++)
            {
                var image = images[i];
                if (image.Length <= 0)
                    continue;

                string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                string filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Sinh ID không bị trùng
                string idHinhAnh = $"HA{(baseNumber + i):D5}";

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