using Microsoft.AspNetCore.Mvc;
using QLSuaChuaVaLapDat.Models;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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

        public IActionResult thongKe(int? year, int? month, string searchString, int page = 1) 
        {
            var viewModel = new ThongKeModel();
            DateTime dateFocus;

            if (year.HasValue && month.HasValue && year.Value > 0 && month.Value > 0 && month.Value <= 12)
            {
                try { dateFocus = new DateTime(year.Value, month.Value, 1); }
                catch (ArgumentOutOfRangeException) { dateFocus = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); }
            }
            else { dateFocus = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); }

            viewModel.ChartDateFocus = dateFocus;
            viewModel.CurrentSearchString = searchString; 

            //  biểu đồ doanh thu
            var chartEndDate = dateFocus;
            var chartStartDate = dateFocus.AddMonths(-5);

            var sixMonthsData = _context.DonDichVus
                .Where(d => d.TrangThaiDon.ToLower() == "hoàn thành" &&
                            d.NgayTaoDon.HasValue &&
                            d.NgayTaoDon.Value.Date >= chartStartDate.Date &&
                            d.NgayTaoDon.Value.Date <= new DateTime(chartEndDate.Year, chartEndDate.Month, DateTime.DaysInMonth(chartEndDate.Year, chartEndDate.Month)).Date)
                .GroupBy(d => new { d.NgayTaoDon.Value.Year, d.NgayTaoDon.Value.Month }) 
                .Select(g => new
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    TongDoanhThu = g.Sum(x => x.TongTien ?? 0)
                })
                .ToList(); 


            viewModel.DoanhThuThang.Clear(); 
            for (int i = 0; i < 6; i++)
            {
                var currentMonthForLoop = chartStartDate.AddMonths(i);
                var monthData = sixMonthsData.FirstOrDefault(d => d.Nam == currentMonthForLoop.Year && d.Thang == currentMonthForLoop.Month);

                viewModel.DoanhThuThang.Add(new DoanhThuThangViewModel
                {
                    Nam = currentMonthForLoop.Year,
                    Thang = currentMonthForLoop.Month,
                    ThangVaNam = $"Thg {currentMonthForLoop.Month} {currentMonthForLoop.Year}",
                    TongDoanhThuTheoThang = monthData?.TongDoanhThu ?? 0 
                                                                         
                });
            }

            decimal maxRevenueInPeriod = viewModel.DoanhThuThang.Any() ? viewModel.DoanhThuThang.Max(r => r.TongDoanhThuTheoThang) : 1;
            if (maxRevenueInPeriod == 0) maxRevenueInPeriod = 1;

            foreach (var doanhThuThang in viewModel.DoanhThuThang)
            {
                doanhThuThang.ChieuCaoCot = (double)(doanhThuThang.TongDoanhThuTheoThang / maxRevenueInPeriod) * 100;
                doanhThuThang.ChieuCaoCot = Math.Max(1, Math.Min(100, doanhThuThang.ChieuCaoCot));
            }

            var ordersInFocusedMonth = _context.DonDichVus
                .Where(d => d.NgayTaoDon.HasValue &&
                            d.NgayTaoDon.Value.Year == dateFocus.Year &&
                            d.NgayTaoDon.Value.Month == dateFocus.Month)
                .ToList();

            var ordersInFocusedYear = _context.DonDichVus
                .Where(d => d.NgayTaoDon.HasValue &&
                            d.NgayTaoDon.Value.Year == dateFocus.Year)
                .ToList();

            viewModel.TongDonCN = ordersInFocusedYear.Count(d => d.TrangThaiDon.ToLower() == "hoàn thành");
            viewModel.ThangHT = dateFocus.ToString("MMMM yyyy", new CultureInfo("vi-VN"));
            viewModel.TongDon = ordersInFocusedMonth.Count(d => d.TrangThaiDon.ToLower() == "hoàn thành");
            viewModel.TongDoanhThuThangHT = ordersInFocusedMonth
                                            .Where(d => d.TrangThaiDon.ToLower() == "hoàn thành")
                                            .Sum(d => d.TongTien ?? 0);

            //  biểu đồ tròn 
            var allRelevantOrdersForPieChart = ordersInFocusedMonth
                .Where(d => d.TrangThaiDon.ToLower() == "hoàn thành").ToList();

            viewModel.KhachHang = allRelevantOrdersForPieChart.Count(d => !string.IsNullOrEmpty(d.IdUser));
            viewModel.KhacVangLai = allRelevantOrdersForPieChart.Count(d => !string.IsNullOrEmpty(d.IdKhachVangLai) && string.IsNullOrEmpty(d.IdUser));
            int totalUserTypeOrders = viewModel.KhachHang + viewModel.KhacVangLai;
            if (totalUserTypeOrders > 0)
            {
                viewModel.PTKhachHang = Math.Round((double)viewModel.KhachHang / totalUserTypeOrders * 100, 1);
                viewModel.PTKhachVangLai = Math.Round((double)viewModel.KhacVangLai / totalUserTypeOrders * 100, 1);
            }
            else {  }


            viewModel.TongDoanhTHu = _context.DonDichVus
                                           .Where(d => d.TrangThaiDon.ToLower() == "hoàn thành")
                                           .Sum(d => d.TongTien ?? 0);
            viewModel.TongNhanVien = _context.Users.Count(u => u.IdRole == "R005");



            int pageSize = 5; 
            IQueryable<DonDichVu> queryDonDichVu = _context.DonDichVus
                                                       .Include(d => d.IdUserNavigation) 
                                                       .Include(d => d.IdKhachVangLaiNavigation) 
                                                       .Include(d => d.IdNhanVienKyThuatNavigation) 
                                                       .OrderByDescending(d => d.NgayTaoDon); 

            if (!String.IsNullOrEmpty(searchString))
            {              
                string lowerSearchString = searchString.ToLower();
                queryDonDichVu = queryDonDichVu.Where(d =>
                    (d.IdDonDichVu != null && d.IdDonDichVu.ToLower().Contains(lowerSearchString)) ||
                    (d.TenThietBi != null && d.TenThietBi.ToLower().Contains(lowerSearchString)) ||
                    (d.IdUserNavigation != null && d.IdUserNavigation.HoVaTen != null && d.IdUserNavigation.HoVaTen.ToLower().Contains(lowerSearchString)) || // Giả sử User có HoTenKhachHang
                    (d.IdKhachVangLaiNavigation != null && d.IdKhachVangLaiNavigation.HoVaTen != null && d.IdKhachVangLaiNavigation.HoVaTen.ToLower().Contains(lowerSearchString)) || // Giả sử KhachVangLai có TenKhach
                    (d.IdNhanVienKyThuatNavigation != null && d.IdNhanVienKyThuatNavigation.HoVaTen != null && d.IdNhanVienKyThuatNavigation.HoVaTen.ToLower().Contains(lowerSearchString)) // Tìm theo tên NV Kỹ thuật
                );
            }

            int totalItems = queryDonDichVu.Count();
            viewModel.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            viewModel.CurrentPage = page;

            viewModel.DonDichVuGanNhat = queryDonDichVu
                                        .Skip((page - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToList();

            return View(viewModel);
        }


        // chi tiet hoa don
        public IActionResult ChiTietDoanhThuThang(int year, int month)
        {
            var viewModel = new ChiTietThangViewModel
            {
                ThangVaNam = new DateTime(year, month, 1).ToString("MMMM yyyy", new CultureInfo("vi-VN"))
            };

            var completedOrdersInMonth = _context.DonDichVus
                .Where(d => d.TrangThaiDon.ToLower() == "hoàn thành" &&
                            d.NgayHoanThanh.HasValue && 
                            d.NgayHoanThanh.Value.Year == year &&
                            d.NgayHoanThanh.Value.Month == month)
                .Include(d => d.IdUserNavigation) 
                .Include(d => d.IdKhachVangLaiNavigation) 
                .ToList();

            var customerSpending = new Dictionary<string, decimal>();

            foreach (var order in completedOrdersInMonth)
            {
                string customerName = "Không xác định";
                if (order.IdUserNavigation != null)
                {
                    customerName = order.IdUserNavigation.HoVaTen; 
                }
                else if (order.IdKhachVangLaiNavigation != null)
                {
                    customerName = order.IdKhachVangLaiNavigation.HoVaTen; 
                }


                if (customerSpending.ContainsKey(customerName))
                {
                    customerSpending[customerName] += order.TongTien ?? 0;
                }
                else
                {
                    customerSpending[customerName] = order.TongTien ?? 0;
                }
            }

            viewModel.ThongTinkh = customerSpending
                .Select(kvp => new ThongTinKhachHangViewModel
                {
                    Ten = kvp.Key,
                    SoLuongTieu = kvp.Value
                })
                .OrderByDescending(kh => kh.SoLuongTieu) 
                .ToList();

            return View(viewModel);
        }
    }
}