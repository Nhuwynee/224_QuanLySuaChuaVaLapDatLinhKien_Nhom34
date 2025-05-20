using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSuaChuaVaLapDat.Models;
using QLSuaChuaVaLapDat.Models.Impl;

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


        public async Task<IActionResult> TimKiemLinhKien()
        {
            var result = await _context.LinhKiens
                .Include(d => d.IdLoaiLinhKienNavigation)
                .Include(d => d.IdNsxNavigation)
                .ToListAsync();

            return View(result);
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
