using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QLSuaChuaVaLapDat.Models;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;

namespace QLSuaChuaVaLapDat.Controllers
{
    public class ExcelExportController : Controller
    {
        private readonly QuanLySuaChuaVaLapDatContext _context;

        public ExcelExportController(QuanLySuaChuaVaLapDatContext context)
        {
            _context = context; 
            // Đăng ký giấy phép EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        [HttpGet]
        public IActionResult ExportDoanhThu(int? fromYear, int? fromMonth, int? toYear, int? toMonth, string exportType)
        {
            try
            {
                DateTime startDate;
                DateTime endDate;

                if (exportType == "range" && fromYear.HasValue && fromMonth.HasValue && toYear.HasValue && toMonth.HasValue)
                {
                    // Xuất theo khoảng thời gian
                    startDate = new DateTime(fromYear.Value, fromMonth.Value, 1);
                    endDate = new DateTime(toYear.Value, toMonth.Value, DateTime.DaysInMonth(toYear.Value, toMonth.Value));
                }
                else if (exportType == "single" && fromYear.HasValue && fromMonth.HasValue)
                {
                    // Xuất theo một tháng cụ thể
                    startDate = new DateTime(fromYear.Value, fromMonth.Value, 1);
                    endDate = startDate.AddMonths(1).AddDays(-1);
                }
                else
                {
                    // Mặc định là tháng hiện tại
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    endDate = startDate.AddMonths(1).AddDays(-1);
                }

                // Lấy dữ liệu đơn dịch vụ trong khoảng thời gian
                var donDichVus = _context.DonDichVus
                    .Where(d => d.NgayTaoDon.HasValue &&
                                d.NgayTaoDon.Value.Date >= startDate.Date &&
                                d.NgayTaoDon.Value.Date <= endDate.Date &&
                                d.TrangThaiDon.ToLower() == "hoàn thành")
                    .OrderBy(d => d.NgayTaoDon)
                    .ToList();

                using (var package = new ExcelPackage())
                {
                    // Tạo worksheet
                    var worksheet = package.Workbook.Worksheets.Add("Doanh Thu");

                    // Thiết lập tiêu đề
                    string title = exportType == "range"
                        ? $"BÁO CÁO DOANH THU TỪ THÁNG {fromMonth}/{fromYear} ĐẾN THÁNG {toMonth}/{toYear}"
                        : $"BÁO CÁO DOANH THU THÁNG {fromMonth}/{fromYear}";

                    worksheet.Cells[1, 1].Value = title;
                    worksheet.Cells[1, 1, 1, 8].Merge = true;
                    worksheet.Cells[1, 1, 1, 8].Style.Font.Bold = true;
                    worksheet.Cells[1, 1, 1, 8].Style.Font.Size = 16;
                    worksheet.Cells[1, 1, 1, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Thiết lập header
                    worksheet.Cells[3, 1].Value = "STT";
                    worksheet.Cells[3, 2].Value = "Mã đơn";
                    worksheet.Cells[3, 3].Value = "Ngày tạo";
                    worksheet.Cells[3, 4].Value = "Ngày hoàn thành";
                    worksheet.Cells[3, 5].Value = "Khách hàng";
                    worksheet.Cells[3, 6].Value = "Thiết bị";
                    worksheet.Cells[3, 7].Value = "Loại dịch vụ";
                    worksheet.Cells[3, 8].Value = "Tổng tiền (VNĐ)";

                    // Định dạng header
                    using (var range = worksheet.Cells[3, 1, 3, 8])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                        range.Style.Font.Color.SetColor(Color.Black);
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }

                    // Điền dữ liệu
                    int row = 4;
                    decimal tongDoanhThu = 0;

                    for (int i = 0; i < donDichVus.Count; i++)
                    {
                        var don = donDichVus[i];
                        string tenKhachHang = "Không xác định";

                        // Lấy tên khách hàng
                        if (don.IdUser != null)
                        {
                            var user = _context.Users.FirstOrDefault(u => u.IdUser == don.IdUser);
                            if (user != null)
                            {
                                tenKhachHang = user.HoVaTen;
                            }
                        }
                        else if (don.IdKhachVangLai != null)
                        {
                            var khachVangLai = _context.KhachVangLais.FirstOrDefault(k => k.IdKhachVangLai == don.IdKhachVangLai);
                            if (khachVangLai != null)
                            {
                                tenKhachHang = khachVangLai.HoVaTen;
                            }
                        }

                        worksheet.Cells[row, 1].Value = i + 1;
                        worksheet.Cells[row, 2].Value = don.IdDonDichVu;
                        worksheet.Cells[row, 3].Value = don.NgayTaoDon?.ToString("dd/MM/yyyy HH:mm");
                        worksheet.Cells[row, 4].Value = don.NgayHoanThanh?.ToString("dd/MM/yyyy HH:mm");
                        worksheet.Cells[row, 5].Value = tenKhachHang;
                        worksheet.Cells[row, 6].Value = don.TenThietBi;
                        worksheet.Cells[row, 7].Value = don.LoaiDonDichVu;
                        worksheet.Cells[row, 8].Value = don.TongTien;

                        // Định dạng số tiền
                        worksheet.Cells[row, 8].Style.Numberformat.Format = "#,##0";

                        // Định dạng border
                        using (var range = worksheet.Cells[row, 1, row, 8])
                        {
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        }

                        tongDoanhThu += don.TongTien ?? 0;
                        row++;
                    }

                    // Thêm dòng tổng
                    worksheet.Cells[row, 1, row, 7].Merge = true;
                    worksheet.Cells[row, 1].Value = "TỔNG CỘNG";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 8].Value = tongDoanhThu;
                    worksheet.Cells[row, 8].Style.Font.Bold = true;
                    worksheet.Cells[row, 8].Style.Numberformat.Format = "#,##0";

                    // Định dạng border cho dòng tổng
                    using (var range = worksheet.Cells[row, 1, row, 8])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Double;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 242, 204));
                    }

                    // Tự động điều chỉnh độ rộng cột
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Tạo tên file
                    string fileName = exportType == "range"
                        ? $"DoanhThu_{fromMonth}-{fromYear}_den_{toMonth}-{toYear}.xlsx"
                        : $"DoanhThu_{fromMonth}-{fromYear}.xlsx";

                    // Trả về file Excel
                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                return RedirectToAction("thongKe", "QuanLI", new { error = "Có lỗi xảy ra khi xuất file: " + ex.Message });
            }
        }
    }
}
