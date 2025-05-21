using Microsoft.AspNetCore.Mvc;
using QLSuaChuaVaLapDat.Models;
using System.Globalization;
using System.Linq;

namespace QLSuaChuaVaLapDat.Controllers
{

    public class QuanLIController : Controller
    {

        private readonly QuanLySuaChuaVaLapDatContext _context;

        public QuanLIController(QuanLySuaChuaVaLapDatContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var User = _context.Users.ToList();
            return View(User);
        }

        public IActionResult thongKe()
        {
            var viewModel = new ThongKeModel();
            var HomNay = DateTime.Today;

            // --- Revenue Chart Data (Example: Last 6 months) ---
            var sixMonthsAgo = HomNay.AddMonths(-5).AddDays(-HomNay.Day + 1); // Start of the month, 6 months ago

            var DonHoanThanh = _context.DonDichVus
                                    .Where(d => d.TrangThaiDon.ToLower() == "hoàn thành" &&
                                                d.NgayTaoDon >= sixMonthsAgo && d.NgayTaoDon <= HomNay)
                                    .ToList();

            var DlDoanhThuThang = DonHoanThanh
                .GroupBy(d => new { Nam = d.NgayTaoDon.Value.Year, Thang = d.NgayTaoDon.Value.Month })
                .Select(g => new DoanhThuThangViewModel
                {
                    Nam = g.Key.Nam,
                    Thang = g.Key.Thang,
                    ThangVaNam = new DateTime(g.Key.Nam, g.Key.Thang, 1).ToString("MMM yyyy", new CultureInfo("vi-VN")), // Changed "MMM Букмекерлар" to "MMM yyyy" for standard date format
                    TongDoanhThuTheoThang = g.Sum(x => x.TongTien ?? 0)
                })
                .OrderBy(r => r.Nam)
                .ThenBy(r => r.Thang)
                .ToList();

            // Calculate maxRevenue after all monthly sums are available
            decimal maxRevenue = DlDoanhThuThang.Any() ? DlDoanhThuThang.Max(r => r.TongDoanhThuTheoThang) : 0; // Start with 0 if no data
            if (maxRevenue == 0) maxRevenue = 1; // Prevent division by zero, set to 1 if max is 0

            foreach (var doanhThuThang in DlDoanhThuThang)
            {
                // Ensure ChieuCaoCot is a valid percentage (0-100)
                if (maxRevenue > 0) // Double-check to prevent division by zero, though maxRevenue should be 1 if it was 0
                {
                    doanhThuThang.ChieuCaoCot = (double)(doanhThuThang.TongDoanhThuTheoThang / maxRevenue) * 100;
                    // Cap the height between 0 and 100, just in case of floating point inaccuracies or weird edge cases
                    doanhThuThang.ChieuCaoCot = Math.Max(1, Math.Min(100, doanhThuThang.ChieuCaoCot));
                }
                else
                {
                    doanhThuThang.ChieuCaoCot = 0; // If maxRevenue is somehow still 0 (shouldn't happen with the check above), set height to 0
                }
                viewModel.DoanhThuThang.Add(doanhThuThang);
            }

            // --- Visitors Pie Chart Data ---
            var allRelevantOrders = _context.DonDichVus
                                            .Where(d => d.TrangThaiDon.ToLower() == "hoàn thành")
                                            .ToList();

            viewModel.KhachHang = allRelevantOrders.Count(d => !string.IsNullOrEmpty(d.IdUser));
            viewModel.KhacVangLai = allRelevantOrders.Count(d => !string.IsNullOrEmpty(d.IdKhachVangLai) && string.IsNullOrEmpty(d.IdUser));

            int totalUserTypeOrders = viewModel.KhachHang + viewModel.KhacVangLai;
            if (totalUserTypeOrders > 0)
            {
                viewModel.PTKhachHang = Math.Round((double)viewModel.KhachHang / totalUserTypeOrders * 100, 1);
                viewModel.PTKhachVangLai = Math.Round((double)viewModel.KhacVangLai / totalUserTypeOrders * 100, 1);
            }
            else
            {
                viewModel.PTKhachHang = 0;
                viewModel.PTKhachVangLai = 0;
            }

            // Calculate TongDon for the current month (Tháng 5 2025)
            // You need to ensure ThongKeModel has TongDoanhThuThangHT property.
            // If HomNay is May 20, 2025, it will count orders for May 2025.
            viewModel.TongDon = _context.DonDichVus.Count(d =>
                d.TrangThaiDon.ToLower() == "hoàn thành" &&
                d.NgayTaoDon.HasValue && // Added null check for NgayTaoDon
                d.NgayTaoDon.Value.Month == HomNay.Month &&
                d.NgayTaoDon.Value.Year == HomNay.Year);

            // Calculate TongDoanhThuThangHT for the current month
            viewModel.TongDoanhThuThangHT = _context.DonDichVus
                .Where(d => d.TrangThaiDon.ToLower() == "hoàn thành" &&
                            d.NgayTaoDon.HasValue && // Added null check for NgayTaoDon
                            d.NgayTaoDon.Value.Month == HomNay.Month &&
                            d.NgayTaoDon.Value.Year == HomNay.Year)
                .Sum(d => d.TongTien ?? 0);

            viewModel.ThangHT = HomNay.ToString("MMMM yyyy", new CultureInfo("vi-VN")); // Changed "MMM Букмекерлар" to "MMMM yyyy" for full month name
            viewModel.TongDoanhTHu = _context.DonDichVus
                                            .Where(d => d.TrangThaiDon.ToLower() == "hoàn thành")
                                            .Sum(d => d.TongTien ?? 0);
            viewModel.TongNhanVien = _context.Users.Count(u => u.IdRole == "R005");


            return View(viewModel);
        }
    }
}