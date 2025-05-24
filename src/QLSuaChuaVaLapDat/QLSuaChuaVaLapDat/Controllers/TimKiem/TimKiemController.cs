using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSuaChuaVaLapDat.Models;
using QLSuaChuaVaLapDat.Models.Impl;
using QLSuaChuaVaLapDat.Models.TimKiem;
using QLSuaChuaVaLapDat.ViewModel;

namespace QLSuaChuaVaLapDat.Controllers.TimKiem
{
    public class TimKiemController : Controller
    {
        private readonly AppDbContext _context;

        public TimKiemController(AppDbContext context) 
        {
            _context = context;
        }

        public async Task<IActionResult> TimKiem()
        {
            var result = await _context.Phuongs
                .Include(d => d.IdQuanNavigation)
                .Include(d => d.IdThanhPhoNavigation)
                .ToListAsync();

            return View(result);
        }
        public async Task<IActionResult> TimKiemDonDichVu()
        {
            var result = await _context.DonDichVus
                .Include(d=>d.ChiTietDonDichVus)
                .Include(d => d.IdUserTaoDonNavigation)
                .Include(d => d.IdKhachVangLaiNavigation)
                .Include(d => d.IdLoaiThietBiNavigation)
                .ToListAsync();
            return View(result);
        }

        public async Task<IActionResult> TimKiemKhachHang()
        {
            // TvT
            var dsKH = await _context.Users
                 .Where(u => u.IdUser.StartsWith("KH"))
                 .Include(u => u.IdPhuongNavigation)
                     .ThenInclude(p => p.IdQuanNavigation)
                         .ThenInclude(q => q.IdThanhPhoNavigation)
                 .GroupJoin(
                     _context.DonDichVus,
                     user => user.IdUser,
                     order => order.IdUser,
                     (user, orders) => new
                     {
                         User = user,
                         TongGiaTri = orders.Sum(o => o.TongTien)
                     })
                 .Select(result => new NguoiDung
                 {
                     IdUser = result.User.IdUser,
                     TenUser = result.User.TenUser,
                     HoVaTen = result.User.HoVaTen,
                     Sdt = result.User.Sdt,
                     NgaySinh = result.User.NgaySinh,
                     TrangThai = result.User.TrangThai,
                     DiaChi = result.User.DiaChi+ result.User.IdPhuongNavigation != null ? result.User.IdPhuongNavigation.TenPhuong : null
                     + result.User.IdPhuongNavigation != null && result.User.IdPhuongNavigation.IdQuanNavigation != null
                         ? result.User.IdPhuongNavigation.IdQuanNavigation.TenQuan: null
                     + result.User.IdPhuongNavigation != null && result.User.IdPhuongNavigation.IdQuanNavigation != null
                         && result.User.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation != null
                         ? result.User.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation.TenThanhPho: null,
                     tongDonHang = (double)result.TongGiaTri
                 }).ToListAsync();


            var dsKVL = await _context.KhachVangLais
                    .Include(d => d.IdPhuongNavigation)
                    .ThenInclude(d => d.IdQuanNavigation)
                    .ThenInclude(d => d.IdThanhPhoNavigation)
                .ToListAsync();

            KhachHang dsKhachHang = new KhachHang();
            dsKhachHang.KhachVangLais = dsKVL;
            dsKhachHang.Users = dsKH;

            return View(dsKhachHang);
        }

        [HttpGet]
        public async Task<IActionResult> TimKiemLinhKien( )
        {
            var resultLinhKien = await _context.LinhKiens
                .Include(d => d.IdLoaiLinhKienNavigation)
                .Include(d => d.IdNsxNavigation)
                .ToListAsync();

            var resultNSX = await _context.NhaSanXuats.ToListAsync();
            var resultLoaiLK = await _context.LoaiLinhKiens.ToListAsync();

            TimKiemLinhKiemVM timKiemLinhKien = new TimKiemLinhKiemVM();
            timKiemLinhKien.LinhKiens = resultLinhKien;
            timKiemLinhKien.NhaSanXuats = resultNSX;
            timKiemLinhKien.LoaiLinhKiens = resultLoaiLK;
            return View(timKiemLinhKien);
        }

        [HttpPost]
        public async Task<IActionResult> TimKiemLinhKien(TimKiemLinhKien timKiemLinhKien)
        {
            var query = _context.LinhKiens
                .Include(d => d.IdLoaiLinhKienNavigation)
                .Include(d => d.IdNsxNavigation)
                .AsQueryable();
            // Lọc theo tình trạng sản phẩm(hết/còn)
            if(timKiemLinhKien.TTSanPham!=null  )
            {
                if(timKiemLinhKien.TTSanPham == 1)
                    query = query.Where(l => l.SoLuong >= 1);
                else
                    query = query.Where(l => l.SoLuong == 0);

            }
           

            // Lọc theo mã linh kiện
            if (!string.IsNullOrEmpty(timKiemLinhKien.MaLinhKien))
            {
                query = query.Where(l => l.IdLinhKien == timKiemLinhKien.MaLinhKien);
            }
            // Lọc theo tên linh kiện
            if (!string.IsNullOrEmpty(timKiemLinhKien.TenLinhKien))
            {
                query = query.Where(l => l.TenLinhKien.Contains(timKiemLinhKien.TenLinhKien));
            }

            // Lọc theo loại linh kiện
            if (!string.IsNullOrEmpty(timKiemLinhKien.LoaiLinhKien))
            {
                query = query.Where(l => l.IdLoaiLinhKienNavigation.TenLoaiLinhKien == timKiemLinhKien.LoaiLinhKien);
            }

            // Lọc theo nhà sản xuất
            if (!string.IsNullOrEmpty(timKiemLinhKien.NhaSanXuat))
            {
                query = query.Where(l => l.IdNsxNavigation.IdNsx == timKiemLinhKien.NhaSanXuat);
            }

            // Lọc theo giá từ và đến
            if (timKiemLinhKien.GiaTu.HasValue)
            {
                if (timKiemLinhKien.GiaTu != 0) { 
                    query = query.Where(l => l.Gia >= timKiemLinhKien.GiaTu);
                }
            }
            if (timKiemLinhKien.GiaDen.HasValue)
            {
                if (timKiemLinhKien.GiaDen != 0)
                {
                    query = query.Where(l => l.Gia <= timKiemLinhKien.GiaDen);
                }
            }

            // Lọc theo trạng thái bảo hành
            if (timKiemLinhKien.BaoHanh!=null)
            {
                query = query.Where(l => l.ThoiGianBaoHanh == timKiemLinhKien.BaoHanh);
            }

            // Sắp xếp
            if (!string.IsNullOrEmpty(timKiemLinhKien.SapXep))
            {
                switch (timKiemLinhKien.SapXep.ToLower())
                {
                    case "asc":
                        query = query.OrderBy(l => l.TenLinhKien);
                        break;
                    case "desc":
                        query = query.OrderByDescending(l => l.TenLinhKien);
                        break;
                    case "price_asc":
                        query = query.OrderBy(l => l.Gia);
                        break;
                    case "price_desc":
                        query = query.OrderByDescending(l => l.Gia);
                        break;
                    case "warranty_asc":
                        query = query.OrderBy(l => l.ThoiGianBaoHanh);
                        break;
                    case "warranty_desc":
                        query = query.OrderByDescending(l => l.ThoiGianBaoHanh);
                        break;
                }
            }

            var resultLinhKien = await query.ToListAsync();

            var resultNSX = await _context.NhaSanXuats.ToListAsync();
            var resultLoaiLK = await _context.LoaiLinhKiens.ToListAsync();

            TimKiemLinhKiemVM timKiemLinhKienVM = new TimKiemLinhKiemVM();
            timKiemLinhKienVM.LinhKiens = resultLinhKien;
            timKiemLinhKienVM.NhaSanXuats = resultNSX;
            timKiemLinhKienVM.LoaiLinhKiens = resultLoaiLK;
            return View(timKiemLinhKienVM);
        }


        public async Task<IActionResult> TimKiemBaoHanh()
        {
            try
            {
                var result = await _context.DonDichVus
                    .Include(d=>d.IdKhachVangLaiNavigation)
                    .Include(d=>d.IdUserNavigation)
                    .Include(d => d.ChiTietDonDichVus)
                        .ThenInclude(ct => ct.IdLinhKienNavigation)
                    .Include(d => d.ChiTietDonDichVus)
                    .ThenInclude(ct => ct.IdLoiNavigation)
                    .ToListAsync();

                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }


        // Các action khác có thể sử dụng _context
    }
}
