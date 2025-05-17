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
            
            return Json(new { 
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
        public IActionResult CreateServiceOrder([FromForm] DonDichVu donDichVu)
        {
            try
            {
                // Check if the ID was provided and is valid
                if (string.IsNullOrEmpty(donDichVu.IdDonDichVu))
                {
                    // If no ID was provided, generate a new one
                    donDichVu.IdDonDichVu = GenerateNextServiceOrderId();
                }
                
                // Set creation date to now if not provided
                if (donDichVu.NgayTaoDon == null)
                {
                    donDichVu.NgayTaoDon = DateTime.Now;
                }
                
                // Set initial status if not provided
                if (string.IsNullOrEmpty(donDichVu.TrangThaiDon))
                {
                    donDichVu.TrangThaiDon = "Chờ xử lý";
                }
                
                // Add the service order to the database
                _context.DonDichVus.Add(donDichVu);
                _context.SaveChanges();
                
                return Json(new { success = true, message = "Đơn dịch vụ đã được tạo thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

    }
}
