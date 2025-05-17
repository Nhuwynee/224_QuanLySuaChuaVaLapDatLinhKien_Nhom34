using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSuaChuaVaLapDat.Models;

namespace QLSuaChuaVaLapDat.Controllers.TimKiem
{
    public class TimKiemController : Controller
    {
        private readonly AppDbContext _context;

        public TimKiemController(AppDbContext context) // ✅ Inject từ DI
        {
            _context = context;
        }

        public async Task<IActionResult> TimKiem()
        {
            var result = await _context.DonDichVus
                .Include(d=>d.ChiTietDonDichVus)
                .Include(d => d.IdUserTaoDonNavigation)
                .Include(d => d.IdKhachVangLaiNavigation)
                .Include(d => d.IdLoaiThietBiNavigation)
                .ToListAsync();

            return View(result);
        }

        // Các action khác có thể sử dụng _context
    }
}
