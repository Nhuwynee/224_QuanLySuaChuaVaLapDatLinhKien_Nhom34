using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using QLSuaChuaVaLapDat.Models;
using QLSuaChuaVaLapDat.Models.TimKiem;
using QLSuaChuaVaLapDat.ViewModel;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using PdfSharpCore.Drawing.Layout;


namespace QLSuaChuaVaLapDat.Controllers.TimKiem
{
    public class TimKiemController : Controller
    {
        private readonly QuanLySuaChuaVaLapDatContext _context;

        public TimKiemController(QuanLySuaChuaVaLapDatContext context) 
        {
            _context = context;
        }

        PdfPage CreateNewPage(PdfDocument doc, out XGraphics gfx, out XTextFormatter tf)
        {
            var newPage = doc.AddPage();
            newPage.Size = PdfSharpCore.PageSize.A4;
            gfx = XGraphics.FromPdfPage(newPage);
            tf = new XTextFormatter(gfx);
            return newPage;
        }

        public async Task<IActionResult> TimKiem()
        {
            var result = await _context.Phuongs
                .Include(d => d.IdQuanNavigation)
                .Include(d => d.IdThanhPhoNavigation)
                .ToListAsync();

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> TimKiemDonDichVu()
        {
            Paging paging = new Paging();
            int pageIndex = paging.PageActive;
            int pageSize = paging.PageSize;

            int totalRecords = await _context.DonDichVus.CountAsync();
            int totalPage = (int)Math.Ceiling((double)totalRecords / pageSize);

            paging.TotalPage = totalPage;

            var ddv = await _context.DonDichVus
                .Include(d => d.ChiTietDonDichVus)
                .Include(d => d.IdUserNavigation)
                .Include(d => d.IdKhachVangLaiNavigation)
                .Include(d => d.IdLoaiThietBiNavigation)
                .Skip((pageIndex - 1) * pageSize) 
                .Take(pageSize)
                .ToListAsync();

            var loaiTB = await _context.ThietBis.ToListAsync();

            TimKiemDichVuVM resultView = new TimKiemDichVuVM();

            resultView.DonDichVu = ddv;
            resultView.Paging = paging;
            resultView.loaiTB = loaiTB;
            DonDichVuSearch donDichVuSearchNew = new DonDichVuSearch();
            resultView.donDichVuSearch = donDichVuSearchNew;
            return View(resultView);

        }

        [HttpPost]
        public async Task<IActionResult> TimKiemDonDichVu(DonDichVuSearch donDichVuSearch)
        {
            int pageIndex = donDichVuSearch.PageActive > 0 ? donDichVuSearch.PageActive : 1;
            int pageSize = 5;

            IQueryable<DonDichVu> query = _context.DonDichVus
                .Include(d => d.ChiTietDonDichVus)
                .Include(d => d.IdUserNavigation)
                .Include(d => d.IdNhanVienKyThuatNavigation)
                .Include(d => d.IdKhachVangLaiNavigation)
                .Include(d => d.IdLoaiThietBiNavigation);

           
            // Áp dụng filter
            if (!string.IsNullOrEmpty(donDichVuSearch.MaDonDichVu))
            {
                query = query.Where(d => d.IdDonDichVu.Contains(donDichVuSearch.MaDonDichVu));
            }

            if (!string.IsNullOrEmpty(donDichVuSearch.IDKyThuatVien))
            {
                query = query.Where(d => d.IdNhanVienKyThuat.Contains(donDichVuSearch.IDKyThuatVien));
            }

            if (!string.IsNullOrEmpty(donDichVuSearch.TrangThaiDV))
            {
               
                query = query.Where(d => d.TrangThaiDon == donDichVuSearch.TrangThaiDV);
            }

            if (!string.IsNullOrEmpty(donDichVuSearch.TuNgay) && DateTime.TryParse(donDichVuSearch.TuNgay, out var tuNgay))
            {
                query = query.Where(d => d.NgayTaoDon >= tuNgay);
            }

            if (!string.IsNullOrEmpty(donDichVuSearch.DenNgay) && DateTime.TryParse(donDichVuSearch.DenNgay, out var denNgay))
            {
                query = query.Where(d => d.NgayTaoDon <= denNgay);
            }

            if (!string.IsNullOrEmpty(donDichVuSearch.IdLoaiThietBi))
            {
                query = query.Where(d => d.IdLoaiThietBiNavigation.IdLoaiThietBi == donDichVuSearch.IdLoaiThietBi);
            }

            if (!string.IsNullOrEmpty(donDichVuSearch.LoaiDichVu))
            {
                query = query.Where(d => d.LoaiDonDichVu == donDichVuSearch.LoaiDichVu);
            }



            if (!string.IsNullOrEmpty(donDichVuSearch.SapXepTheoIdDonDichVu))
            {
                query = donDichVuSearch.SapXepTheoIdDonDichVu switch
                {
                    "IdDonDichVuAsc" => query.OrderBy(d => d.IdDonDichVu),
                    "IdDonDichVuDesc" => query.OrderByDescending(d => d.IdDonDichVu),
                    _ => query.OrderByDescending(d => d.NgayTaoDon) 
                };
            }
           if (!string.IsNullOrEmpty(donDichVuSearch.SapXepTheoTenKhachHang))
            {
                query = donDichVuSearch.SapXepTheoTenKhachHang switch
                {
                    "TenKhachHangAsc" => query.OrderBy(d => d.IdUserNavigation != null ? d.IdUserNavigation.TenUser : d.IdKhachVangLaiNavigation.HoVaTen),
                    "TenKhachHangDesc" => query.OrderByDescending(d => d.IdUserNavigation != null ? d.IdUserNavigation.TenUser : d.IdKhachVangLaiNavigation.HoVaTen),
                    _ => query.OrderByDescending(d => d.NgayTaoDon)
                };
            }
            if (!string.IsNullOrEmpty(donDichVuSearch.SapXepTheoTongTien))
            {
                query = donDichVuSearch.SapXepTheoTongTien switch
                {
                    "TongTienAsc" => query.OrderBy(d => d.TongTien),
                    "TongTienDesc" => query.OrderByDescending(d => d.TongTien),
                    _ => query.OrderByDescending(d => d.NgayTaoDon)
                };
            }
            if (!string.IsNullOrEmpty(donDichVuSearch.SapXepTheoNgayTao))
            {
                query = donDichVuSearch.SapXepTheoNgayTao switch
                {
                    "NgayTaoAsc" => query.OrderBy(d => d.NgayTaoDon),
                    "NgayTaoDesc" => query.OrderByDescending(d => d.NgayTaoDon),
                    _ => query.OrderByDescending(d => d.NgayTaoDon)
                };
            }
            

            var pagedResult = await query.ToListAsync();

            // Lọc theo tên khách hàng
            if (!string.IsNullOrEmpty(donDichVuSearch.TenKhachHang))
            {
                var keyword = RemoveDiacritics(donDichVuSearch.TenKhachHang.ToLower());// khach
                pagedResult = pagedResult.Where(d =>
                    (d.IdKhachVangLaiNavigation != null &&
                     d.IdKhachVangLaiNavigation.HoVaTen != null &&
                     RemoveDiacritics(d.IdKhachVangLaiNavigation.HoVaTen.ToLower()).Contains(keyword)) ||

                    (d.IdUserNavigation != null &&
                     d.IdUserNavigation.HoVaTen != null &&
                     RemoveDiacritics(d.IdUserNavigation.HoVaTen.ToLower()).Contains(keyword))
                ).ToList();
            }


            // Export to Excel
            if (donDichVuSearch.isexport == 1)
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Thêm dòng này
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("DonDichVu");

                    // Set title row
                    worksheet.Cells[1, 1].Value = "Thông Tin Đơn Dịch Vụ";  
                    worksheet.Cells[1, 1, 1, 9].Merge = true; 
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 1].Style.Font.Size = 17;
                    worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; 

                    // Set header row
                    worksheet.Cells[2, 1].Value = "STT";
                    worksheet.Cells[2, 2].Value = "Mã đơn";
                    worksheet.Cells[2, 3].Value = "Khách hàng";
                    worksheet.Cells[2, 4].Value = "Thiết bị";
                    worksheet.Cells[2, 5].Value = "Loại dịch vụ";
                    worksheet.Cells[2, 6].Value = "Trạng thái";
                    worksheet.Cells[2, 7].Value = "Tổng tiền";
                    worksheet.Cells[2, 8].Value = "Ngày tạo";
                    worksheet.Cells[2, 9].Value = "Ngày cập nhật";

                    using (var range = worksheet.Cells[2, 1, 2, 9]) 
                    {
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue); 
                        range.Style.Font.Bold = true; 
                    }

                    // Populate data rows
                    for (int i = 0; i < pagedResult.Count; i++)
                    {
                        var record = pagedResult[i];
                        worksheet.Cells[i + 3, 1].Value = i + 1; // Start data from row 3
                        worksheet.Cells[i + 3, 2].Value = record.IdDonDichVu ?? "N/A";
                        worksheet.Cells[i + 3, 3].Value = record.IdUserNavigation?.HoVaTen ?? record.IdKhachVangLaiNavigation?.HoVaTen ?? "N/A";
                        worksheet.Cells[i + 3, 4].Value = record.IdLoaiThietBiNavigation?.TenLoaiThietBi ?? "N/A";
                        worksheet.Cells[i + 3, 5].Value = record.LoaiDonDichVu ?? "N/A";
                        worksheet.Cells[i + 3, 6].Value = record.TrangThaiDon ?? "N/A";
                        worksheet.Cells[i + 3, 7].Value = record.TongTien?.ToString("N0") ?? "0";
                        worksheet.Cells[i + 3, 8].Value = record.NgayTaoDon?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
                        worksheet.Cells[i + 3, 9].Value = record.NgayChinhSua?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
                    }

                    
                    int footerRow = pagedResult.Count + 3; 
                    worksheet.Cells[footerRow, 1].Value = $"Ngày xuất file:{DateTime.Now.ToString("dd/MM/yyyy HH:mm")}";
                    worksheet.Cells[footerRow, 1, footerRow, 3].Merge = true;
                    worksheet.Cells[footerRow, 5].Value = $"Tổng số lượng: {pagedResult.Count}";
                    worksheet.Cells[footerRow, 5, footerRow, 7].Merge = true; 
                    worksheet.Cells[footerRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[footerRow, 5].Style.Font.Bold = true;

                    
                    worksheet.Cells.AutoFitColumns();

                    // Generate filename with current date
                    string fileName = $"DonDichVu_{DateTime.Now:yyyyMMdd}.xlsx";

                    // Return the Excel file
                    var stream = new MemoryStream(package.GetAsByteArray());
                    ViewBag.IsExport = 0;
                    ViewBag.IsBaoCao = 0;
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }

            // Export to Report
            if (donDichVuSearch.isBaoCao == 1)
            {
                var doc = new PdfDocument();
                var page = doc.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;
                var gfx = XGraphics.FromPdfPage(page);

                // Fonts
                var font = new XFont("Arial", 9, XFontStyle.Regular);
                var fontBold = new XFont("Arial", 10, XFontStyle.Bold);
                var titleFont = new XFont("Arial", 14, XFontStyle.Bold);
                var headerFont = new XFont("Arial", 11, XFontStyle.Bold);
                var pen = new XPen(XColors.Black, 0.5);

                // Layout
                double margin = 40;
                double y = margin;
                double rowHeight = 22;
                double pageHeight = page.Height - 2 * margin; // Available height for content
                double pageWidth = page.Width - 2 * margin;

                // Method to add a new page and reset y
                void AddNewPage()
                {
                    page = doc.AddPage();
                    page.Size = PdfSharpCore.PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                }

                // Header: Quốc hiệu
                gfx.DrawString("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM", headerFont, XBrushes.Black, new XRect(0, y, page.Width, 15), XStringFormats.TopCenter);
                y += 15;
                gfx.DrawString("Độc lập - Tự do - Hạnh phúc", font, XBrushes.Black, new XRect(0, y, page.Width, 15), XStringFormats.TopCenter);
                y += 25;

                // Report Title
                gfx.DrawString("ĐIỆN MÁY XANH", headerFont, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("Địa chỉ: 222 Nguyễn Văn Linh, Phường Thạc Gián, Quận Thanh Khê, Tp Đà Nẵng", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("Thời gian hoạt động: 7 giờ 30 phút - 22 giờ (kể cả CN và ngày lễ)", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("Số điện thoại: 1900232461", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("BÁO CÁO ĐƠN DỊCH VỤ", titleFont, XBrushes.DarkRed, new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);
                y += 30;

                // Thông tin người lập báo cáo
                gfx.DrawString("Họ và tên: .........................................................", font, XBrushes.Black, new XPoint(margin, y));
                gfx.DrawString("Chức vụ: ................................", font, XBrushes.Black, new XPoint(page.Width / 2 + 20, y));
                y += 15;

                gfx.DrawString($"Từ ngày: {donDichVuSearch.TuNgay?.ToString() ?? "..."}  đến ngày  {donDichVuSearch.DenNgay?.ToString() ?? "..."}",
                    font, XBrushes.Black, new XPoint(margin, y));
                y += 15;

                gfx.DrawString($"Trạng thái đơn: {donDichVuSearch.TrangThaiDV ?? "Tất cả"}", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;

                gfx.DrawString($"Theo kỹ thuật viên: {donDichVuSearch.IDKyThuatVien?.ToString() ?? "..."}", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;

                gfx.DrawString("Bộ phận công tác: .........................................................", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;

                gfx.DrawString($"Thời gian thực hiện: Ngày {DateTime.Now:dd} Tháng {DateTime.Now:MM} Năm {DateTime.Now:yyyy}",
                    font, XBrushes.Black, new XPoint(margin, y));
                y += 25;

                // Tiêu đề bảng
                gfx.DrawString("I. DANH SÁCH ĐƠN DỊCH VỤ", fontBold, XBrushes.Black, new XPoint(margin, y));
                y += 20;

                // Header bảng
                double[] colWidths = { 30, 50, 90, 95, 60, 70, 70, 70 };
                string[] headers = { "STT", "Mã đơn", "Khách hàng", "Thiết bị", "Loại DV", "Trạng thái", "Tổng tiền", "Ngày tạo" };
                double colX = margin;

                for (int i = 0; i < headers.Length; i++)
                {
                    gfx.DrawRectangle(pen, XBrushes.LightGray, colX, y, colWidths[i], rowHeight);
                    gfx.DrawString(headers[i], fontBold, XBrushes.Black, new XRect(colX + 3, y + 5, colWidths[i], rowHeight), XStringFormats.TopLeft);
                    colX += colWidths[i];
                }
                y += rowHeight;

                // Dữ liệu bảng with page break
                int maxRows = pagedResult.Count;
                for (int i = 0; i < maxRows; i++)
                {
                    // Check if we need a new page
                    if (y + rowHeight > pageHeight)
                    {
                        AddNewPage();
                        // Redraw headers on new page
                        colX = margin;
                        for (int h = 0; h < headers.Length; h++)
                        {
                            gfx.DrawRectangle(pen, XBrushes.LightGray, colX, y, colWidths[h], rowHeight);
                            gfx.DrawString(headers[h], fontBold, XBrushes.Black, new XRect(colX + 3, y + 5, colWidths[h], rowHeight), XStringFormats.TopLeft);
                            colX += colWidths[h];
                        }
                        y += rowHeight;
                    }

                    var item = pagedResult[i];
                    string[] values = {
                        (i + 1).ToString(),
                        item.IdDonDichVu ?? "N/A",
                        item.IdUserNavigation?.HoVaTen ?? item.IdKhachVangLaiNavigation?.HoVaTen ?? "N/A",
                        item.IdLoaiThietBiNavigation?.TenLoaiThietBi ?? "N/A",
                        item.LoaiDonDichVu ?? "N/A",
                        item.TrangThaiDon ?? "N/A",
                        item.TongTien?.ToString("N0") ?? "0",
                        item.NgayTaoDon?.ToString("dd/MM/yyyy") ?? "N/A"
                    };

                    colX = margin;
                    for (int j = 0; j < values.Length; j++)
                    {
                        // Truncate text if it exceeds column width
                        string displayText = values[j];
                        int maxChar = (int)(colWidths[j] / (gfx.MeasureString("A", font).Width / 1.5));
                        if (gfx.MeasureString(displayText, font).Width > colWidths[j])
                        {
                            int safeLength = Math.Min(displayText.Length, maxChar);
                            displayText = displayText.Substring(0, safeLength) + "...";
                        }
                        gfx.DrawRectangle(pen, colX, y, colWidths[j], rowHeight);
                        gfx.DrawString(displayText, font, XBrushes.Black, new XRect(colX + 3, y + 5, colWidths[j], rowHeight), XStringFormats.TopLeft);
                        colX += colWidths[j];
                    }
                    y += rowHeight;
                }

                // Ghi chú & chữ ký
                if (y + 55 > pageHeight) // Ensure footer fits on a new page if needed
                {
                    AddNewPage();
                }
                y += 20;
                gfx.DrawString($"Tổng số lượng đơn dịch vụ: {pagedResult.Count}", fontBold, XBrushes.Black, new XPoint(margin, y));
                y += 20;
                gfx.DrawString("Ghi chú: Báo cáo có hiệu lực trong vòng 7 ngày kể từ ngày xuất báo cáo.", font, XBrushes.Black, new XPoint(margin, y));
                y += 35;
                gfx.DrawString("PHỤ TRÁCH BỘ PHẬN", fontBold, XBrushes.Black, new XPoint(margin + 30, y));
                gfx.DrawString("NGƯỜI BÁO CÁO", fontBold, XBrushes.Black, new XPoint(page.Width - margin - 130, y));

                // Xuất PDF
                using var stream = new MemoryStream();
                doc.Save(stream, false);
                string fileName = $"BaoCaoDonDichVu{DateTime.Now:yyyyMMddHHmm}.pdf"; // Changed to .pdf
                ViewBag.IsExport = 0;
                ViewBag.IsBaoCao = 0;
                return File(stream.ToArray(), "application/pdf", fileName);
            }
            
            int totalRecords = pagedResult.Count;
             int totalPage = (int)Math.Ceiling((double)totalRecords / pageSize);

  
            List<DonDichVu> donDichVu = pagedResult
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();


            var loaiTB = await _context.ThietBis.ToListAsync();
            TimKiemDichVuVM resultView = new TimKiemDichVuVM();
            resultView.DonDichVu = donDichVu;
            resultView.loaiTB = loaiTB;
            Paging paging = new Paging();
            paging.TotalPage = totalPage;
            paging.PageActive = pageIndex;
            DonDichVuSearch donDichVuSearchNew = new DonDichVuSearch();
            donDichVuSearchNew = donDichVuSearch;
            ViewBag.IsExport = 0;
            ViewBag.IsBaoCao = 0;
            resultView.donDichVuSearch = donDichVuSearchNew;

            resultView.Paging = paging;
            return View(resultView);

        }
        private string RemoveDiacritics(string input)
        {
            var normalizedString = input.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        [HttpGet]
        public async Task<IActionResult> TimKiemKhachHang()
        {
            Paging paging = new Paging();
            int currentPage = 1; 
            int pageSize = paging.PageSize;

            // Query Users (khách hàng đã đăng ký)
            var queryUsers = _context.Users
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
                        TongGiaTri = orders.Sum(o => o.TongTien),
                        TongDon = orders.Count()
                    });

            var dsKH = await queryUsers
                .Select(result => new KhachHangViewTimKiem
                {
                    MaKH = result.User.IdUser,
                    TenDangKy = result.User.TenUser,
                    TenKhachHang = result.User.HoVaTen,
                    SoDienThoai = result.User.Sdt,
                    NgaySinh = result.User.NgaySinh,
                    TrangThai = result.User.TrangThai,
                    DiaChi = (result.User.DiaChi ?? "") +
                             (result.User.IdPhuongNavigation != null ? ", " + result.User.IdPhuongNavigation.TenPhuong : "") +
                             (result.User.IdPhuongNavigation != null && result.User.IdPhuongNavigation.IdQuanNavigation != null
                                 ? ", " + result.User.IdPhuongNavigation.IdQuanNavigation.TenQuan : "") +
                             (result.User.IdPhuongNavigation != null && result.User.IdPhuongNavigation.IdQuanNavigation != null
                                 && result.User.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation != null
                                 ? ", " + result.User.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation.TenThanhPho : ""),
                    TongSoTienSuaChua = (double)result.TongGiaTri,
                    TongSoDon = result.TongDon
                }).ToListAsync();

            // Query KhachVangLais (khách vãng lai)
            var queryKVL = _context.KhachVangLais
                .Include(k => k.IdPhuongNavigation)
                    .ThenInclude(p => p.IdQuanNavigation)
                        .ThenInclude(q => q.IdThanhPhoNavigation)
                .GroupJoin(
                    _context.DonDichVus,
                    khachVangLai => khachVangLai.IdKhachVangLai,
                    order => order.IdKhachVangLai,
                    (khachVangLai, orders) => new
                    {
                        KhachVangLai = khachVangLai,
                        TongGiaTri = orders.Sum(o => o.TongTien),
                        TongDon = orders.Count()
                    });

            var dsKVL = await queryKVL
                .Select(result => new KhachHangViewTimKiem
                {
                    MaKH = result.KhachVangLai.IdKhachVangLai,
                    TenDangKy = null,
                    TenKhachHang = result.KhachVangLai.HoVaTen,
                    SoDienThoai = result.KhachVangLai.Sdt,
                    NgaySinh = null,
                    TrangThai = null,
                    DiaChi = (result.KhachVangLai.DiaChi ?? "") +
                             (result.KhachVangLai.IdPhuongNavigation != null ? ", " + result.KhachVangLai.IdPhuongNavigation.TenPhuong : "") +
                             (result.KhachVangLai.IdPhuongNavigation != null && result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation != null
                                 ? ", " + result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation.TenQuan : "") +
                             (result.KhachVangLai.IdPhuongNavigation != null && result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation != null
                                 && result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation != null
                                 ? ", " + result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation.TenThanhPho : ""),
                    TongSoTienSuaChua = (double)result.TongGiaTri,
                    TongSoDon = null
                }).ToListAsync();

            // Gộp danh sách
            var allCustomers = dsKH.Concat(dsKVL).ToList();

            // Tính tổng số bản ghi và phân trang
            int totalRecords = allCustomers.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var pagedCustomers = allCustomers
                .OrderBy(k => k.MaKH) // Hoặc thứ tự khác nếu bạn muốn
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Lấy danh sách địa phương
            var ThanhPhos = _context.ThanhPhos
                .Select(tp => new ThanhPhoDTO
                {
                    IdThanhPho = tp.IdThanhPho,
                    TenThanhPho = tp.TenThanhPho
                }).ToList();

            var Quans = _context.Quans
                .Select(q => new QuanDTO
                {
                    IdQuan = q.IdQuan,
                    IdThanhPho = q.IdThanhPho,
                    TenQuan = q.TenQuan
                }).ToList();

            var Phuongs = _context.Phuongs
                .Select(p => new PhuongDTO
                {
                    IdPhuong = p.IdPhuong,
                    IdQuan = p.IdQuan,
                    IdThanhPho = p.IdThanhPho,
                    TenPhuong = p.TenPhuong
                }).ToList();

            // Tạo ViewModel
            KhachHangSearchVM viewKH = new KhachHangSearchVM
            {
                Phuongs = Phuongs,
                Quans = Quans,
                ThanhPhos = ThanhPhos,
                KhachHangs = pagedCustomers,
                Paging = new Paging
                {
                    PageActive = currentPage,
                    TotalPage = totalPages,
                    PageSize = pageSize
                }
            };

            return View(viewKH);
        }



        [HttpPost]
        public async Task<IActionResult> TimKiemKhachHang(KhachHangSearch khachHangSearch)
        {
            Paging paging = new Paging();
            int pageUser = khachHangSearch.pageActive ==0? 1: khachHangSearch.pageActive;
            int pageSize = paging.PageSize;

            // Query for registered users (Users)
            var userQuery = _context.Users
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
                        TongGiaTri = orders.Sum(o => o.TongTien ?? 0),
                        TongDon = orders.Count()
                    })
                .AsQueryable();

            // Query for guest customers (KhachVangLai)
            var khachVangLaiQuery = _context.KhachVangLais
                .Include(k => k.IdPhuongNavigation)
                    .ThenInclude(p => p.IdQuanNavigation)
                        .ThenInclude(q => q.IdThanhPhoNavigation)
                .GroupJoin(
                    _context.DonDichVus,
                    khachVangLai => khachVangLai.IdKhachVangLai,
                    order => order.IdKhachVangLai,
                    (khachVangLai, orders) => new
                    {
                        KhachVangLai = khachVangLai,
                        TongGiaTri = orders.Sum(o => o.TongTien ?? 0)
                    })
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(khachHangSearch.SoDienThoai))
            {
                userQuery = userQuery.Where(u => u.User.Sdt.Contains(khachHangSearch.SoDienThoai));
                khachVangLaiQuery = khachVangLaiQuery.Where(k => k.KhachVangLai.Sdt.Contains(khachHangSearch.SoDienThoai));
            }

            if (!string.IsNullOrEmpty(khachHangSearch.ThanhPho))
            {
                userQuery = userQuery.Where(u => u.User.IdPhuongNavigation != null &&
                                                 u.User.IdPhuongNavigation.IdQuanNavigation != null &&
                                                 u.User.IdPhuongNavigation.IdQuanNavigation.IdThanhPho == khachHangSearch.ThanhPho);
                khachVangLaiQuery = khachVangLaiQuery.Where(k => k.KhachVangLai.IdPhuongNavigation != null &&
                                                                 k.KhachVangLai.IdPhuongNavigation.IdQuanNavigation != null &&
                                                                 k.KhachVangLai.IdPhuongNavigation.IdQuanNavigation.IdThanhPho == khachHangSearch.ThanhPho);
            }

            if (!string.IsNullOrEmpty(khachHangSearch.QuanHuyen))
            {
                userQuery = userQuery.Where(u => u.User.IdPhuongNavigation != null &&
                                                 u.User.IdPhuongNavigation.IdQuan == khachHangSearch.QuanHuyen);
                khachVangLaiQuery = khachVangLaiQuery.Where(k => k.KhachVangLai.IdPhuongNavigation != null &&
                                                                 k.KhachVangLai.IdPhuongNavigation.IdQuan == khachHangSearch.QuanHuyen);
            }

            if (!string.IsNullOrEmpty(khachHangSearch.PhuongXa))
            {
                userQuery = userQuery.Where(u => u.User.IdPhuong == khachHangSearch.PhuongXa);
                khachVangLaiQuery = khachVangLaiQuery.Where(k => k.KhachVangLai.IdPhuong == khachHangSearch.PhuongXa);
            }

            // Execute queries
            var dsKH = await userQuery
                .Select(result => new KhachHangViewTimKiem
                {
                    MaKH = result.User.IdUser,
                    TenDangKy = result.User.TenUser,
                    TenKhachHang = result.User.HoVaTen,
                    SoDienThoai = result.User.Sdt,
                    NgaySinh = result.User.NgaySinh,
                    TrangThai = result.User.TrangThai,
                    DiaChi = result.User.DiaChi +
                             (result.User.IdPhuongNavigation != null ? ", " + result.User.IdPhuongNavigation.TenPhuong : "") +
                             (result.User.IdPhuongNavigation != null && result.User.IdPhuongNavigation.IdQuanNavigation != null
                                 ? ", " + result.User.IdPhuongNavigation.IdQuanNavigation.TenQuan : "") +
                             (result.User.IdPhuongNavigation != null && result.User.IdPhuongNavigation.IdQuanNavigation != null
                                 && result.User.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation != null
                                 ? ", " + result.User.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation.TenThanhPho : ""),
                    TongSoTienSuaChua = (double)result.TongGiaTri,
                    TongSoDon = result.TongDon
                })
                .ToListAsync();

            var dsKVL = await khachVangLaiQuery
                .Select(result => new KhachHangViewTimKiem
                {
                    MaKH = result.KhachVangLai.IdKhachVangLai,
                    TenDangKy = null,
                    TenKhachHang = result.KhachVangLai.HoVaTen,
                    SoDienThoai = result.KhachVangLai.Sdt,
                    DiaChi = result.KhachVangLai.DiaChi +
                             (result.KhachVangLai.IdPhuongNavigation != null ? ", " + result.KhachVangLai.IdPhuongNavigation.TenPhuong : "") +
                             (result.KhachVangLai.IdPhuongNavigation != null && result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation != null
                                 ? ", " + result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation.TenQuan : "") +
                             (result.KhachVangLai.IdPhuongNavigation != null && result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation != null
                                 && result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation != null
                                 ? ", " + result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation.TenThanhPho : ""),
                    TongSoDon = null,
                    TongSoTienSuaChua = (double)result.TongGiaTri,
                    NgaySinh = null
                })
                .ToListAsync();

            var danhSachGop = dsKH.Concat(dsKVL).ToList();

            // Apply additional filters
            if (!string.IsNullOrEmpty(khachHangSearch.LoaiKhachHang))
            {
                danhSachGop = khachHangSearch.LoaiKhachHang == "Users"
                    ? danhSachGop.Where(k => k.MaKH.StartsWith("KH")).ToList()
                    : danhSachGop.Where(k => !k.MaKH.StartsWith("KH")).ToList();
            }

            if (!string.IsNullOrEmpty(khachHangSearch.TenKhachHang))
            {
                string keyword = RemoveDiacritics(khachHangSearch.TenKhachHang.ToLower());
                danhSachGop = danhSachGop.Where(k => RemoveDiacritics(k.TenKhachHang?.ToLower() ?? "").Contains(keyword)).ToList();
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(khachHangSearch.SapXepTheoMaKH))
            {
                danhSachGop = khachHangSearch.SapXepTheoMaKH switch
                {
                    "MaKHAsc" => danhSachGop.OrderBy(k => k.MaKH).ToList(),
                    "MaKHDesc" => danhSachGop.OrderByDescending(k => k.MaKH).ToList(),
                    _ => danhSachGop.OrderBy(k => k.MaKH).ToList()
                };
            }
            if (!string.IsNullOrEmpty(khachHangSearch.SapXepTheoTenUser))
            {
                danhSachGop = khachHangSearch.SapXepTheoTenUser switch
                {
                    "TenUserAsc" => danhSachGop.OrderBy(k => k.TenDangKy).ToList(),
                    "TenUserDesc" => danhSachGop.OrderByDescending(k => k.TenDangKy).ToList(),
                    _ => danhSachGop.OrderBy(k => k.TenDangKy).ToList()
                };
            }
            if (!string.IsNullOrEmpty(khachHangSearch.SapXepTheoTenKhachHang))
            {
                danhSachGop = khachHangSearch.SapXepTheoTenKhachHang switch
                {
                    "TenKhachHangAsc" => danhSachGop.OrderBy(k => k.TenKhachHang).ToList(),
                    "TenKhachHangDesc" => danhSachGop.OrderByDescending(k => k.TenKhachHang).ToList(),
                    _ => danhSachGop
                };
            }
            if (!string.IsNullOrEmpty(khachHangSearch.SapXepTheoTongSoDon))
            {
                danhSachGop = khachHangSearch.SapXepTheoTongSoDon switch
                {
                    "TongSoDonAsc" => danhSachGop.OrderBy(k => k.TongSoDon ?? 0).ToList(),
                    "TongSoDonDesc" => danhSachGop.OrderByDescending(k => k.TongSoDon ?? 0).ToList(),
                    _ => danhSachGop
                };
            }
            if (!string.IsNullOrEmpty(khachHangSearch.SapXepTheoTongSoTienSuaChua))
            {
                danhSachGop = khachHangSearch.SapXepTheoTongSoTienSuaChua switch
                {
                    "TongSoTienSuaChuaAsc" => danhSachGop.OrderBy(k => k.TongSoTienSuaChua).ToList(),
                    "TongSoTienSuaChuaDesc" => danhSachGop.OrderByDescending(k => k.TongSoTienSuaChua).ToList(),
                    _ => danhSachGop
                };
            }

            // Excel Export
            if (khachHangSearch.isexport == 1)
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Khách hàng");

                // Title
                worksheet.Cells[1, 1].Value = "Thông Tin Khách Hàng";
                worksheet.Cells[1, 1, 1, 10].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 17;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // Headers
                worksheet.Cells[2, 1].Value = "STT";
                worksheet.Cells[2, 2].Value = "Mã khách hàng";
                worksheet.Cells[2, 3].Value = "Tên đăng ký";
                worksheet.Cells[2, 4].Value = "Tên khách hàng";
                worksheet.Cells[2, 5].Value = "Số điện thoại";
                worksheet.Cells[2, 6].Value = "Ngày sinh";
                worksheet.Cells[2, 7].Value = "Trạng thái";
                worksheet.Cells[2, 8].Value = "Tổng số đơn";
                worksheet.Cells[2, 9].Value = "Tổng tiền sửa chữa";
                worksheet.Cells[2, 10].Value = "Địa chỉ";

                using (var range = worksheet.Cells[2, 1, 2, 10])
                {
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    range.Style.Font.Bold = true;
                }

                // Data
                for (int i = 0; i < danhSachGop.Count; i++)
                {
                    var record = danhSachGop[i];
                    worksheet.Cells[i + 3, 1].Value = i + 1;
                    worksheet.Cells[i + 3, 2].Value = record.MaKH;
                    worksheet.Cells[i + 3, 3].Value = record.TenDangKy ?? "N/A";
                    worksheet.Cells[i + 3, 4].Value = record.TenKhachHang ?? "N/A";
                    worksheet.Cells[i + 3, 5].Value = record.SoDienThoai ?? "N/A";
                    worksheet.Cells[i + 3, 6].Value = record.NgaySinh?.ToString("dd/MM/yyyy") ?? "N/A";
                    worksheet.Cells[i + 3, 7].Value = record.TrangThai == true ? "Hoạt động": "Khóa" ?? "N/A";
                    worksheet.Cells[i + 3, 8].Value = record.TongSoDon?.ToString() ?? "0";
                    worksheet.Cells[i + 3, 9].Value = record.TongSoTienSuaChua?.ToString("N0");
                    worksheet.Cells[i + 3, 10].Value = record.DiaChi ?? "N/A";
                }

                // Footer
                int footerRow = danhSachGop.Count + 3;
                worksheet.Cells[footerRow, 1].Value = $"Ngày xuất file: {DateTime.Now:dd/MM/yyyy HH:mm}";
                worksheet.Cells[footerRow, 1, footerRow, 3].Merge = true;
                worksheet.Cells[footerRow, 5].Value = $"Tổng số lượng: {danhSachGop.Count}";
                worksheet.Cells[footerRow, 5, footerRow, 7].Merge = true;
                worksheet.Cells[footerRow, 1].Style.Font.Bold = true;
                worksheet.Cells[footerRow, 5].Style.Font.Bold = true;

                worksheet.Cells.AutoFitColumns();

                string fileName = $"KhachHang_{DateTime.Now:yyyyMMdd}.xlsx";
                var stream = new MemoryStream(package.GetAsByteArray());
                ViewBag.IsExport = 0;
                ViewBag.IsBaoCao = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }

            // PDF Report
            if (khachHangSearch.isBaoCao == 1)
            {
                var doc = new PdfDocument();
                var page = doc.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;
                var gfx = XGraphics.FromPdfPage(page);

                // Fonts
                var font = new XFont("Arial", 9, XFontStyle.Regular);
                var fontBold = new XFont("Arial", 10, XFontStyle.Bold);
                var titleFont = new XFont("Arial", 14, XFontStyle.Bold);
                var headerFont = new XFont("Arial", 11, XFontStyle.Bold);
                var pen = new XPen(XColors.Black, 0.5);

                // Layout
                double margin = 40;
                double y = margin;
                double rowHeight = 22;
                double pageWidth = page.Width - 2 * margin;

                // Header
                gfx.DrawString("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM", headerFont, XBrushes.Black, new XRect(0, y, page.Width, 15), XStringFormats.TopCenter);
                y += 15;
                gfx.DrawString("Độc lập - Tự do - Hạnh phúc", font, XBrushes.Black, new XRect(0, y, page.Width, 15), XStringFormats.TopCenter);
                y += 25;

                // Report Title
                gfx.DrawString("ĐIỆN MÁY XANH", headerFont, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("Địa chỉ: 222 Nguyễn Văn Linh, Phường Thạc Gián, Quận Thanh Khê, Tp Đà Nẵng", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("Thời gian hoạt động: 7 giờ 30 phút - 22 giờ (kể cả CN và ngày lễ)", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("Số điện thoại: 1900232461", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("BÁO CÁO KHÁCH HÀNG", titleFont, XBrushes.DarkRed, new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);
                y += 30;

                // Report Info
                gfx.DrawString("Họ và tên: .........................................................", font, XBrushes.Black, new XPoint(margin, y));
                gfx.DrawString("Chức vụ: ................................", font, XBrushes.Black, new XPoint(page.Width / 2 + 20, y));
                y += 15;
                gfx.DrawString($"Loại khách hàng: {(khachHangSearch.LoaiKhachHang == null ? "Tất cả" : (khachHangSearch.LoaiKhachHang == "KhachVangLais" ? "Khách vãng lai" : "Khách hàng"))}", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("Bộ phận công tác: .........................................................", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString($"Thời gian thực hiện: Ngày {DateTime.Now:dd} Tháng {DateTime.Now:MM} Năm {DateTime.Now:yyyy}", font, XBrushes.Black, new XPoint(margin, y));
                y += 25;

                // Table Title
                gfx.DrawString("I. DANH SÁCH KHÁCH HÀNG", fontBold, XBrushes.Black, new XPoint(margin, y));
                y += 20;

                // Table Headers
                double[] colWidths = { 25, 50, 100, 80, 100, 60, 60, 60 };
                string[] headers = { "STT", "Mã KH", "Tên KH", "SĐT", "Địa chỉ", "Tổng đơn", "Tổng tiền", "Trạng thái" };
                double colX = margin;

                for (int i = 0; i < headers.Length; i++)
                {
                    gfx.DrawRectangle(pen, XBrushes.LightGray, colX, y, colWidths[i], rowHeight);
                    gfx.DrawString(headers[i], fontBold, XBrushes.Black, new XRect(colX + 3, y + 5, colWidths[i], rowHeight), XStringFormats.TopLeft);
                    colX += colWidths[i];
                }
                y += rowHeight;

                // Table Data
                int maxRows = danhSachGop.Count;
                var tf = new XTextFormatter(gfx);
                double pageHeight = page.Height;
                double bottomMargin = 60;

                for (int i = 0; i < maxRows; i++)
                {
                    var item = danhSachGop[i];
                    string[] values = {
                        (i + 1).ToString(),
                        item.MaKH,
                        item.TenKhachHang ?? "N/A",
                        item.SoDienThoai ?? "N/A",
                        item.DiaChi ?? "N/A",
                        item.TongSoDon?.ToString() ?? "0",
                        item.TongSoTienSuaChua?.ToString("N0"),
                        item.TrangThai == true ? "Hoạt động" : "Không hoạt động"
                    };

                    colX = margin;

                    // Ước lượng chiều cao dòng
                    double rowMaxHeight = rowHeight;
                    for (int j = 0; j < values.Length; j++)
                    {
                        var size = gfx.MeasureString(values[j], font);
                        int lineCount = (int)Math.Ceiling(size.Width / (colWidths[j] - 6));
                        rowMaxHeight = Math.Max(rowMaxHeight, rowHeight * Math.Min(3, lineCount));
                    }

                    // Nếu không đủ chỗ trên trang hiện tại → sang trang mới
                    if (y + rowMaxHeight + bottomMargin > pageHeight)
                    {
                        // Trang mới
                        page = CreateNewPage(doc, out gfx, out tf);
                        y = margin;

                        // Vẽ lại tiêu đề bảng (header row)
                        colX = margin;
                        for (int j = 0; j < headers.Length; j++)
                        {
                            gfx.DrawRectangle(pen, XBrushes.LightGray, colX, y, colWidths[j], rowHeight);
                            gfx.DrawString(headers[j], fontBold, XBrushes.Black, new XRect(colX + 3, y + 5, colWidths[j], rowHeight), XStringFormats.TopLeft);
                            colX += colWidths[j];
                        }
                        y += rowHeight;
                    }

                    // Vẽ dòng dữ liệu
                    colX = margin;
                    for (int j = 0; j < values.Length; j++)
                    {
                        gfx.DrawRectangle(pen, colX, y, colWidths[j], rowMaxHeight);
                        var rect = new XRect(colX + 3, y + 3, colWidths[j] - 6, rowMaxHeight - 6);
                        //tf.DrawString(values[j], font, XBrushes.Black, rect, XStringFormats.TopLeft);
                        tf.DrawString(values[j], font, XBrushes.Black, rect);
                        colX += colWidths[j];
                    }
                    y += rowMaxHeight;
                }


                // Footer
                y += 20;
                gfx.DrawString($"Tổng số khách hàng: {danhSachGop.Count}", fontBold, XBrushes.Black, new XPoint(margin, y));
                y += 20;
                gfx.DrawString("Ghi chú: Báo cáo có hiệu lực trong vòng 7 ngày kể từ ngày xuất báo cáo.", font, XBrushes.Black, new XPoint(margin, y));
                y += 35;
                gfx.DrawString("PHỤ TRÁCH BỘ PHẬN", fontBold, XBrushes.Black, new XPoint(margin + 30, y));
                gfx.DrawString("NGƯỜI BÁO CÁO", fontBold, XBrushes.Black, new XPoint(page.Width - margin - 130, y));

                // Save PDF
                using var stream = new MemoryStream();
                doc.Save(stream, false);
                string fileName = $"BaoCaoKhachHang_{DateTime.Now:yyyyMMddHHmm}.pdf";
                ViewBag.IsExport = 0;
                ViewBag.IsBaoCao = 0;
                return File(stream.ToArray(), "application/pdf", fileName);
            }

            // Pagination
            int totalRecords = danhSachGop.Count;
            int totalPage = (int)Math.Ceiling((double)totalRecords / pageSize);

            List<KhachHangViewTimKiem> danhSachPhanTrang = danhSachGop
                .Skip((pageUser - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            paging.TotalPage = totalPage;
            paging.PageActive = khachHangSearch.pageActive;

            // Load dropdown data
            var thanhPhos = await _context.ThanhPhos
                .Select(tp => new ThanhPhoDTO
                {
                    IdThanhPho = tp.IdThanhPho,
                    TenThanhPho = tp.TenThanhPho
                })
                .ToListAsync();

            var quans = await _context.Quans
                .Select(q => new QuanDTO
                {
                    IdQuan = q.IdQuan,
                    IdThanhPho = q.IdThanhPho,
                    TenQuan = q.TenQuan
                })
                .ToListAsync();

            var phuongs = await _context.Phuongs
                .Select(p => new PhuongDTO
                {
                    IdPhuong = p.IdPhuong,
                    IdQuan = p.IdQuan,
                    IdThanhPho = p.IdThanhPho,
                    TenPhuong = p.TenPhuong
                })
                .ToListAsync();

            var viewKH = new KhachHangSearchVM
            {
                Phuongs = phuongs,
                Quans = quans,
                ThanhPhos = thanhPhos,
                KhachHangs = danhSachPhanTrang,
                Paging = paging,
                KhachHangSearch = khachHangSearch
            };

            return View(viewKH);
        }
        [HttpGet]
        public async Task<IActionResult> TimKiemLinhKien()
        {
            Paging paging = new Paging();
            int pageIndex = paging.PageActive;
            int pageSize = paging.PageSize;

            int totalRecords = await _context.LinhKiens.CountAsync();
            int totalPage = (int)Math.Ceiling((double)totalRecords / pageSize);

            paging.TotalPage = totalPage;

            var resultLinhKien = await _context.LinhKiens
                .Include(d => d.IdLoaiLinhKienNavigation)
                .Include(d => d.IdNsxNavigation)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var resultNSX = await _context.NhaSanXuats.ToListAsync();
            var resultLoaiLK = await _context.LoaiLinhKiens.ToListAsync();

            TimKiemLinhKiemVM timKiemLinhKien = new TimKiemLinhKiemVM();
            timKiemLinhKien.LinhKiens = resultLinhKien;
            timKiemLinhKien.Paging = paging;
            timKiemLinhKien.NhaSanXuats = resultNSX;
            timKiemLinhKien.LoaiLinhKiens = resultLoaiLK;
            LinhKienSearch lkSearchNew = new LinhKienSearch();
            timKiemLinhKien.linhKienSearch = lkSearchNew;
            return View(timKiemLinhKien);
        }

        [HttpPost]
        public async Task<IActionResult> TimKiemLinhKien(LinhKienSearch timKiemLinhKien)
        {
            int pageIndex = timKiemLinhKien.PageActive > 0 ? timKiemLinhKien.PageActive : 1;
            int pageSize = 5;

            var query = _context.LinhKiens
                .Include(d => d.IdLoaiLinhKienNavigation)
                .Include(d => d.IdNsxNavigation)
                .AsQueryable();
            // Lọc theo tình trạng sản phẩm(hết/còn)
            if(timKiemLinhKien.TTSanPham!=null  )
            {
                if(timKiemLinhKien.TTSanPham == 1)
                    query = query.Where(l => l.SoLuong > 0);
                else
                    query = query.Where(l => l.SoLuong == 0);

            }
           

            // Lọc theo mã linh kiện
            if (!string.IsNullOrEmpty(timKiemLinhKien.MaLinhKien))
            {
                query = query.Where(l => l.IdLinhKien == timKiemLinhKien.MaLinhKien);
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

            // Lọc theo tên linh kiện
            if (!string.IsNullOrEmpty(timKiemLinhKien.TenLinhKien))
            {
                string keyword = RemoveDiacritics(timKiemLinhKien.TenLinhKien.ToLower());
                resultLinhKien = resultLinhKien.Where(u => RemoveDiacritics(u.TenLinhKien.ToLower()).Contains(keyword)).ToList();
            }
            if (!string.IsNullOrEmpty(timKiemLinhKien.SapXepTheoMaLinhKien))
            {
                resultLinhKien = timKiemLinhKien.SapXepTheoMaLinhKien == "MaLinhKienAsc"
                    ? resultLinhKien.OrderBy(dto => dto.IdLinhKien).ToList()
                    : resultLinhKien.OrderByDescending(dto => dto.IdLinhKien).ToList();
            }
            if (!string.IsNullOrEmpty(timKiemLinhKien.SapXepTheoTenLinhKien))
            {
                resultLinhKien = timKiemLinhKien.SapXepTheoTenLinhKien == "TenLinhKienAsc"
                    ? resultLinhKien.OrderBy(dto => dto.TenLinhKien).ToList()
                    : resultLinhKien.OrderByDescending(dto => dto.TenLinhKien).ToList();
            }
            if (!string.IsNullOrEmpty(timKiemLinhKien.SapXepTheoLoaiLinhKien))
            {
                resultLinhKien = timKiemLinhKien.SapXepTheoLoaiLinhKien == "LoaiLinhKienAsc"
                    ? resultLinhKien.OrderBy(dto => dto.IdLoaiLinhKienNavigation?.TenLoaiLinhKien).ToList()
                    : resultLinhKien.OrderByDescending(dto => dto.IdLoaiLinhKienNavigation?.TenLoaiLinhKien).ToList();
            }
            if (!string.IsNullOrEmpty(timKiemLinhKien.SapXepTheoNhaSanXuat))
            {
                resultLinhKien = timKiemLinhKien.SapXepTheoNhaSanXuat == "NhaSanXuatAsc"
                    ? resultLinhKien.OrderBy(dto => dto.IdNsxNavigation?.TenNsx).ToList()
                    : resultLinhKien.OrderByDescending(dto => dto.IdNsxNavigation?.TenNsx).ToList();
            }
            if (!string.IsNullOrEmpty(timKiemLinhKien.SapXepTheoGia))
            {
                resultLinhKien = timKiemLinhKien.SapXepTheoGia == "GiaAsc"
                    ? resultLinhKien.OrderBy(dto => dto.Gia).ToList()
                    : resultLinhKien.OrderByDescending(dto => dto.Gia).ToList();
            }
            if (!string.IsNullOrEmpty(timKiemLinhKien.SapXepTheoSoLuong))
            {
                resultLinhKien = timKiemLinhKien.SapXepTheoSoLuong == "SoLuongAsc"
                    ? resultLinhKien.OrderBy(dto => dto.SoLuong).ToList()
                    : resultLinhKien.OrderByDescending(dto => dto.SoLuong).ToList();
            }
            if (!string.IsNullOrEmpty(timKiemLinhKien.SapXepTheoBaoHanh))
            {
                resultLinhKien = timKiemLinhKien.SapXepTheoBaoHanh == "BaoHanhAsc"
                    ? resultLinhKien.OrderBy(dto => dto.ThoiGianBaoHanh).ToList()
                    : resultLinhKien.OrderByDescending(dto => dto.ThoiGianBaoHanh).ToList();
            }


            if (timKiemLinhKien.isexport == 1)
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Linh kiện");

                // Title
                worksheet.Cells[1, 1].Value = "Thông Tin Linh Kiện";
                worksheet.Cells[1, 1, 1, 10].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 17;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // Headers
                worksheet.Cells[2, 1].Value = "STT";
                worksheet.Cells[2, 2].Value = "Mã linh kiện";
                worksheet.Cells[2, 3].Value = "Tên linh kiện";
                worksheet.Cells[2, 4].Value = "Loại linh kiện";
                worksheet.Cells[2, 5].Value = "Nhà sản xuất";
                worksheet.Cells[2, 6].Value = "Giá";
                worksheet.Cells[2, 7].Value = "Số lượng còn";
                worksheet.Cells[2, 8].Value = "Bảo hành (tháng)";
                worksheet.Cells[2, 9].Value = "Tình trạng";
                worksheet.Cells[2, 10].Value = "Điều kiện bảo hành";

                using (var range = worksheet.Cells[2, 1, 2, 10])
                {
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    range.Style.Font.Bold = true;
                }

                // Data
                for (int i = 0; i < resultLinhKien.Count; i++)
                {
                    var record = resultLinhKien[i];
                    worksheet.Cells[i + 3, 1].Value = i + 1;
                    worksheet.Cells[i + 3, 2].Value = record.IdLinhKien;
                    worksheet.Cells[i + 3, 3].Value = record.TenLinhKien ?? "N/A";
                    worksheet.Cells[i + 3, 4].Value = record.IdLoaiLinhKienNavigation?.TenLoaiLinhKien ?? "N/A";
                    worksheet.Cells[i + 3, 5].Value = record.IdNsxNavigation?.TenNsx ?? "N/A";
                    worksheet.Cells[i + 3, 6].Value = record.Gia.ToString("N0") ?? "0";
                    worksheet.Cells[i + 3, 7].Value = record.SoLuong;
                    worksheet.Cells[i + 3, 8].Value = record.ThoiGianBaoHanh+"tháng";
                    worksheet.Cells[i + 3, 9].Value = record.SoLuong > 0 ? "Còn hàng" : "Hết hàng";
                    worksheet.Cells[i + 3, 10].Value = record.DieuKienBaoHanh ?? "N/A";
                }

                // Footer
                int footerRow = resultLinhKien.Count + 3;
                worksheet.Cells[footerRow, 1].Value = $"Ngày xuất file: {DateTime.Now:dd/MM/yyyy HH:mm}"; // 03:57 PM +07 on Monday, June 09, 2025
                worksheet.Cells[footerRow, 1, footerRow, 3].Merge = true;
                worksheet.Cells[footerRow, 5].Value = $"Tổng số lượng: {resultLinhKien.Count}";
                worksheet.Cells[footerRow, 5, footerRow, 7].Merge = true;
                worksheet.Cells[footerRow, 1].Style.Font.Bold = true;
                worksheet.Cells[footerRow, 5].Style.Font.Bold = true;

                worksheet.Cells.AutoFitColumns();

                string fileName = $"LinhKien_{DateTime.Now:yyyyMMdd}.xlsx";
                var stream = new MemoryStream(package.GetAsByteArray());
                ViewBag.IsExport = 0;
                ViewBag.IsBaoCao = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }

            var TenNhaSanXuat = _context.NhaSanXuats.Find(timKiemLinhKien.NhaSanXuat);


            if (timKiemLinhKien.isBaoCao == 1)
            {
                var doc = new PdfDocument();
                var page = doc.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;
                var gfx = XGraphics.FromPdfPage(page);

                // Fonts
                var font = new XFont("Arial", 9, XFontStyle.Regular);
                var fontBold = new XFont("Arial", 10, XFontStyle.Bold);
                var titleFont = new XFont("Arial", 14, XFontStyle.Bold);
                var headerFont = new XFont("Arial", 11, XFontStyle.Bold);
                var pen = new XPen(XColors.Black, 0.5);

                // Layout
                double margin = 40;
                double y = margin;
                double rowHeight = 22;
                double pageWidth = page.Width - 2 * margin;

                // Header
                gfx.DrawString("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM", headerFont, XBrushes.Black, new XRect(0, y, page.Width, 15), XStringFormats.TopCenter);
                y += 15;
                gfx.DrawString("Độc lập - Tự do - Hạnh phúc", font, XBrushes.Black, new XRect(0, y, page.Width, 15), XStringFormats.TopCenter);
                y += 25;

                // Report Title
                gfx.DrawString("ĐIỆN MÁY XANH", headerFont, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("Địa chỉ: 222 Nguyễn Văn Linh, Phường Thạc Gián, Quận Thanh Khê, Tp Đà Nẵng", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("Thời gian hoạt động: 7 giờ 30 phút - 22 giờ (kể cả CN và ngày lễ)", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("Số điện thoại: 1900232461", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("BÁO CÁO LINH KIỆN", titleFont, XBrushes.DarkRed, new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);
                y += 30;

                // Report Info
                gfx.DrawString("Họ và tên: .........................................................", font, XBrushes.Black, new XPoint(margin, y));
                gfx.DrawString("Chức vụ: ................................", font, XBrushes.Black, new XPoint(page.Width / 2 + 20, y));
                y += 15;
                gfx.DrawString($"Loại linh kiện: {(timKiemLinhKien.LoaiLinhKien ?? "Tất cả")}", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString($"Nhà sản xuất: {(TenNhaSanXuat?.TenNsx ==null ? "Tất cả":$"{TenNhaSanXuat?.TenNsx}")}", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString($"Thời gian bảo hành: {(timKiemLinhKien.BaoHanh.HasValue ? $"{timKiemLinhKien.BaoHanh.Value} tháng" : "Tất cả")}", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString($"Tình trạng còn hàng: {(timKiemLinhKien.TTSanPham == null ? "Tất cả" : (timKiemLinhKien.TTSanPham == 0 ? "Hết hàng" : "Còn hàng"))}", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("Bộ phận công tác: .........................................................", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString($"Thời gian thực hiện: Ngày {DateTime.Now:dd} Tháng {DateTime.Now:MM} Năm {DateTime.Now:yyyy}", font, XBrushes.Black, new XPoint(margin, y)); // 03:57 PM +07 on Monday, June 09, 2025
                y += 25;

                // Table Title
                gfx.DrawString("I. DANH SÁCH LINH KIỆN", fontBold, XBrushes.Black, new XPoint(margin, y));
                y += 20;

                // Table Headers
                double[] colWidths = { 30, 50, 80, 50, 60, 40, 50, 50, 50, 80 };
                string[] headers = { "STT", "Mã LK", "Tên LK", "Loại LK", "Nhà SX", "Giá", "Số lượng", "Bảo hành", "Tình trạng", "Điều kiện BH" };
                double colX = margin;

                for (int i = 0; i < headers.Length; i++)
                {
                    gfx.DrawRectangle(pen, XBrushes.LightGray, colX, y, colWidths[i], rowHeight);
                    gfx.DrawString(headers[i], fontBold, XBrushes.Black, new XRect(colX + 3, y + 5, colWidths[i], rowHeight), XStringFormats.TopLeft);
                    colX += colWidths[i];
                }
                y += rowHeight;

                // Table Data
                int maxRows = resultLinhKien.Count;
                var tf = new XTextFormatter(gfx);
                double pageHeight = page.Height;
                double bottomMargin = 60;

                for (int i = 0; i < maxRows; i++)
                {
                    var item = resultLinhKien[i];
                    string[] values = {
                    (i + 1).ToString(),
                    item.IdLinhKien,
                    item.TenLinhKien ?? "N/A",
                    item.IdLoaiLinhKienNavigation?.TenLoaiLinhKien ?? "N/A",
                    item.IdNsxNavigation?.TenNsx ?? "N/A",
                    item.Gia.ToString("N0") ?? "0",
                    item.SoLuong.ToString(),
                    item.ThoiGianBaoHanh.ToString(),
                    item.SoLuong > 0 ? "Còn hàng" : "Hết hàng",
                    item.DieuKienBaoHanh ?? "N/A"
                };

                    colX = margin;

                    // Estimate row height
                    double rowMaxHeight = rowHeight;
                    for (int j = 0; j < values.Length; j++)
                    {
                        var size = gfx.MeasureString(values[j], font);
                        int lineCount = (int)Math.Ceiling(size.Width / (colWidths[j] - 6));
                        rowMaxHeight = Math.Max(rowMaxHeight, rowHeight * Math.Min(3, lineCount));
                    }

                    // If not enough space on the current page, create a new page
                    if (y + rowMaxHeight + bottomMargin > pageHeight)
                    {
                        page = CreateNewPage(doc, out gfx, out tf);
                        y = margin;

                        // Redraw table headers
                        colX = margin;
                        for (int j = 0; j < headers.Length; j++)
                        {
                            gfx.DrawRectangle(pen, XBrushes.LightGray, colX, y, colWidths[j], rowHeight);
                            gfx.DrawString(headers[j], fontBold, XBrushes.Black, new XRect(colX + 3, y + 5, colWidths[j], rowHeight), XStringFormats.TopLeft);
                            colX += colWidths[j];
                        }
                        y += rowHeight;
                    }

                    // Draw data row
                    colX = margin;
                    for (int j = 0; j < values.Length; j++)
                    {
                        gfx.DrawRectangle(pen, colX, y, colWidths[j], rowMaxHeight);
                        var rect = new XRect(colX + 3, y + 3, colWidths[j] - 6, rowMaxHeight - 6);
                        //tf.DrawString(values[j], font, XBrushes.Black, rect, XStringFormats.TopLeft);
                        tf.DrawString(values[j], font, XBrushes.Black, rect);
                        colX += colWidths[j];
                    }
                    y += rowMaxHeight;
                }

                // Footer
                y += 20;
                gfx.DrawString($"Tổng số linh kiện: {resultLinhKien.Count}", fontBold, XBrushes.Black, new XPoint(margin, y));
                y += 20;
                gfx.DrawString("Ghi chú: Báo cáo có hiệu lực trong vòng 7 ngày kể từ ngày xuất báo cáo.", font, XBrushes.Black, new XPoint(margin, y));
                y += 35;
                gfx.DrawString("PHỤ TRÁCH BỘ PHẬN", fontBold, XBrushes.Black, new XPoint(margin + 30, y));
                gfx.DrawString("NGƯỜI BÁO CÁO", fontBold, XBrushes.Black, new XPoint(page.Width - margin - 130, y));

                // Save PDF
                using var stream = new MemoryStream();
                doc.Save(stream, false);
                string fileName = $"BaoCaoLinhKien_{DateTime.Now:yyyyMMddHHmm}.pdf"; 
                ViewBag.IsExport = 0;
                ViewBag.IsBaoCao = 0;
                return File(stream.ToArray(), "application/pdf", fileName);
            }

            int totalRecords = resultLinhKien.Count;
            int totalPage = (int)Math.Ceiling((double)totalRecords / pageSize);


            List<LinhKien> linhKienList = resultLinhKien
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var resultNSX = await _context.NhaSanXuats.ToListAsync();
            var resultLoaiLK = await _context.LoaiLinhKiens.ToListAsync();

            TimKiemLinhKiemVM timKiemLinhKienVM = new TimKiemLinhKiemVM();
            timKiemLinhKienVM.LinhKiens = linhKienList;
            timKiemLinhKienVM.NhaSanXuats = resultNSX;
            timKiemLinhKienVM.LoaiLinhKiens = resultLoaiLK;

            Paging paging = new Paging();
            paging.TotalPage = totalPage;
            paging.PageActive = pageIndex;

            LinhKienSearch lkSearchNew = new LinhKienSearch();
            lkSearchNew = timKiemLinhKien;
            timKiemLinhKienVM.linhKienSearch = lkSearchNew;

            timKiemLinhKienVM.Paging = paging;

            return View(timKiemLinhKienVM);


        }

        [HttpGet]
        public async Task<IActionResult> TimKiemBaoHanh()
        {
            Paging paging = new Paging();
            int pageIndex = paging.PageActive;
            int pageSize = paging.PageSize;

            int totalRecords = await _context.ChiTietDonDichVus.CountAsync();
            int totalPage = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Query ChiTietDonDichVu directly
            var chiTietDonDichVus = await _context.ChiTietDonDichVus
                .Include(ct => ct.IdDonDichVuNavigation)
                    .ThenInclude(d => d.IdKhachVangLaiNavigation)
                .Include(ct => ct.IdDonDichVuNavigation)
                    .ThenInclude(d => d.IdUserNavigation)
                .Include(ct => ct.IdLinhKienNavigation)
                    .ThenInclude(lk => lk.IdNsxNavigation)
                .Include(ct => ct.IdLoiNavigation)
                .ToListAsync();

            var chiTietDonHangDtos = chiTietDonDichVus.Select(ct => new ChiTietDonHangDTO
            {
                IDChiTietDonDichVu = ct.IdCtdh,
                idDonDichVu = ct.IdDonDichVuNavigation.IdDonDichVu,
                MaLinhKien = ct.IdLinhKienNavigation?.IdLinhKien ?? null,
                SDT = ct.IdDonDichVuNavigation.IdKhachVangLaiNavigation?.Sdt ?? ct.IdDonDichVuNavigation.IdUserNavigation?.Sdt ?? "1900 1858",
                MaLoi = ct.IdLoiNavigation?.IdLoi ?? null,
                TenLinhKien = ct.IdLinhKienNavigation?.TenLinhKien ?? "Không có linh kiện", // Add null check
                TenLoi = ct.IdLoiNavigation?.MoTaLoi ?? "Không có lỗi",
                LoaiDichVu = ct.IdDonDichVuNavigation.LoaiDonDichVu ?? "N/A",
                TenKhachHang = ct.IdDonDichVuNavigation.IdKhachVangLaiNavigation?.HoVaTen ?? ct.IdDonDichVuNavigation.IdUserNavigation?.HoVaTen ?? "Khách vãng lai",
                NgayKichHoat = ct.ThoiGianThemLinhKien,
                NgayHetHan = ct.NgayKetThucBh.HasValue
          ? ct.NgayKetThucBh.Value.ToDateTime(TimeOnly.MinValue)
          : (ct.ThoiGianThemLinhKien.HasValue ? ct.ThoiGianThemLinhKien.Value.AddMonths(ct.IdLinhKienNavigation?.ThoiGianBaoHanh ?? 0) : (DateTime?)null),
                TrangThaiBaoHanh = (bool)ct.HanBaoHanh,
                DieuKien = ct.IdLinhKienNavigation?.DieuKienBaoHanh?? null
            }).ToList();

            chiTietDonHangDtos = chiTietDonHangDtos.OrderBy(dto => dto.idDonDichVu)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();


            var loaiLK = await _context.LoaiLinhKiens.ToListAsync();
            var NhaSX = await _context.NhaSanXuats.ToListAsync();

            paging.TotalPage = totalPage;
            paging.PageActive = pageIndex;
            BaoHanhSearch baoHanhSearchNew = new BaoHanhSearch();
            BaoHanhSearchVM baohanhView = new BaoHanhSearchVM
            {
                ChiTietDonHangs = chiTietDonHangDtos,
                nhaSanXuats = NhaSX,
                linhKiens = loaiLK,
                Paging = paging,
                baoHanhSearch = baoHanhSearchNew
            };

            return View(baohanhView);
            
            
        }


        [HttpPost]
        public async Task<IActionResult> TimKiemBaoHanh(BaoHanhSearch baoHanhSearch)
        {
            Paging paging = new Paging();
            int pageIndex = baoHanhSearch.PageActive > 0 ? baoHanhSearch.PageActive : 1;
            int pageSize = paging.PageSize;
            // Query ChiTietDonDichVu directly to simplify mapping
            var query = _context.ChiTietDonDichVus
                .Include(ct => ct.IdDonDichVuNavigation)
                    .ThenInclude(d => d.IdKhachVangLaiNavigation)
                .Include(ct => ct.IdDonDichVuNavigation)
                    .ThenInclude(d => d.IdUserNavigation)
                .Include(ct => ct.IdLinhKienNavigation)
                    .ThenInclude(lk => lk.IdNsxNavigation)
                .Include(ct => ct.IdLoiNavigation)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(baoHanhSearch.MaDonDichVu))
            {
                query = query.Where(ct => ct.IdDonDichVuNavigation.IdDonDichVu.Contains(baoHanhSearch.MaDonDichVu));
            }

            if (!string.IsNullOrEmpty(baoHanhSearch.MaLinhKien))
            {
                query = query.Where(ct => ct.IdLinhKienNavigation.IdLinhKien == baoHanhSearch.MaLinhKien);
            }

            if (!string.IsNullOrEmpty(baoHanhSearch.SoDienThoaiKhachHang))
            {
                query = query.Where(ct => (ct.IdDonDichVuNavigation.IdKhachVangLaiNavigation != null && ct.IdDonDichVuNavigation.IdKhachVangLaiNavigation.Sdt.Contains(baoHanhSearch.SoDienThoaiKhachHang)) ||
                                          (ct.IdDonDichVuNavigation.IdUserNavigation != null && ct.IdDonDichVuNavigation.IdUserNavigation.Sdt.Contains(baoHanhSearch.SoDienThoaiKhachHang)));
            }

            if (baoHanhSearch.TrangThai != null)
            {
                query = query.Where(ct => ct.HanBaoHanh == baoHanhSearch.TrangThai);
            }

            if (baoHanhSearch.TuNgay.HasValue)
            {
                query = query.Where(ct => ct.ThoiGianThemLinhKien >= baoHanhSearch.TuNgay.Value);
            }

            if (baoHanhSearch.DenNgay.HasValue)
            {
                query = query.Where(ct => ct.ThoiGianThemLinhKien <= baoHanhSearch.DenNgay.Value);
            }

            if (baoHanhSearch.LoaiLinhKien != null)
            {
                query = query.Where(ct => ct.IdLinhKienNavigation.IdLoaiLinhKien == baoHanhSearch.LoaiLinhKien);
            }

            if (!string.IsNullOrEmpty(baoHanhSearch.NhaSanXuat))
            {
                query = query.Where(ct => ct.IdLinhKienNavigation.IdNsxNavigation.IdNsx == baoHanhSearch.NhaSanXuat);
            }



            // Fetch the results and map to ChiTietDonHangDTO
            var chiTietDonDichVus = await query.ToListAsync();

            var chiTietDonHangDtos = chiTietDonDichVus.Select(ct => new ChiTietDonHangDTO
            {
                IDChiTietDonDichVu = ct.IdCtdh,
                idDonDichVu = ct.IdDonDichVuNavigation.IdDonDichVu,
                SDT = ct.IdDonDichVuNavigation.IdKhachVangLaiNavigation?.Sdt ?? ct.IdDonDichVuNavigation.IdUserNavigation?.Sdt ?? "1900 1858",
                MaLinhKien = ct.IdLinhKienNavigation?.IdLinhKien ?? null,
                MaLoi = ct.IdLoiNavigation?.IdLoi ?? null,
                TenLinhKien = ct.IdLinhKienNavigation?.TenLinhKien ?? "Không có linh kiện", // Add null check
                TenLoi = ct.IdLoiNavigation?.MoTaLoi ?? "Không có lỗi",
                LoaiDichVu = ct.IdDonDichVuNavigation.LoaiDonDichVu ?? "N/A",
                TenKhachHang = ct.IdDonDichVuNavigation.IdKhachVangLaiNavigation?.HoVaTen ?? ct.IdDonDichVuNavigation.IdUserNavigation?.TenUser ?? "Khách vãng lai",
                NgayKichHoat = ct.ThoiGianThemLinhKien,
                NgayHetHan = ct.NgayKetThucBh.HasValue
           ? ct.NgayKetThucBh.Value.ToDateTime(TimeOnly.MinValue)
           : (ct.ThoiGianThemLinhKien.HasValue ? ct.ThoiGianThemLinhKien.Value.AddMonths(ct.IdLinhKienNavigation?.ThoiGianBaoHanh ?? 0) : (DateTime?)null),
                TrangThaiBaoHanh = (bool)ct.HanBaoHanh,
                DieuKien = ct.IdLinhKienNavigation?.DieuKienBaoHanh ?? null
            }).ToList();



            // Sorting
            if (!string.IsNullOrEmpty(baoHanhSearch.SapXepTheoidDonDichVu))
            {
                chiTietDonHangDtos = baoHanhSearch.SapXepTheoidDonDichVu == "idDonDichVuAsc"
                    ? chiTietDonHangDtos.OrderBy(dto => dto.idDonDichVu).ToList()
                    : chiTietDonHangDtos.OrderByDescending(dto => dto.idDonDichVu).ToList();
            }
            if (!string.IsNullOrEmpty(baoHanhSearch.SapXepTheoIDChiTietDonDichVu))
            {
                chiTietDonHangDtos = baoHanhSearch.SapXepTheoIDChiTietDonDichVu == "IDChiTietDonDichVuAsc"
                    ? chiTietDonHangDtos.OrderBy(dto => dto.IDChiTietDonDichVu).ToList()
                    : chiTietDonHangDtos.OrderByDescending(dto => dto.IDChiTietDonDichVu).ToList();
            }
            if (!string.IsNullOrEmpty(baoHanhSearch.SapXepTheoMaLinhKien))
            {
                chiTietDonHangDtos = baoHanhSearch.SapXepTheoMaLinhKien == "MaLinhKienAsc"
                    ? chiTietDonHangDtos.OrderBy(dto => dto.MaLinhKien).ToList()
                    : chiTietDonHangDtos.OrderByDescending(dto => dto.MaLinhKien).ToList();
            }
            if (!string.IsNullOrEmpty(baoHanhSearch.SapXepTheoTenLinhKien))
            {
                chiTietDonHangDtos = baoHanhSearch.SapXepTheoTenLinhKien == "TenLinhKienAsc"
                    ? chiTietDonHangDtos.OrderBy(dto => dto.TenLinhKien).ToList()
                    : chiTietDonHangDtos.OrderByDescending(dto => dto.TenLinhKien).ToList();
            }
            if (!string.IsNullOrEmpty(baoHanhSearch.SapXepTheoTenKhachHang))
            {
                chiTietDonHangDtos = baoHanhSearch.SapXepTheoTenKhachHang == "TenKhachHangAsc"
                    ? chiTietDonHangDtos.OrderBy(dto => dto.TenKhachHang).ToList()
                    : chiTietDonHangDtos.OrderByDescending(dto => dto.TenKhachHang).ToList();
            }
            if (!string.IsNullOrEmpty(baoHanhSearch.SapXepTheoNgayKichHoat))
            {
                chiTietDonHangDtos = baoHanhSearch.SapXepTheoNgayKichHoat == "NgayKichHoatAsc"
                    ? chiTietDonHangDtos.OrderBy(dto => dto.NgayKichHoat).ToList()
                    : chiTietDonHangDtos.OrderByDescending(dto => dto.NgayKichHoat).ToList();
            }
            if (!string.IsNullOrEmpty(baoHanhSearch.SapXepTheoNgayHetHan))
            {
                chiTietDonHangDtos = baoHanhSearch.SapXepTheoNgayHetHan == "NgayHetHanAsc"
                    ? chiTietDonHangDtos.OrderBy(dto => dto.NgayHetHan).ToList()
                    : chiTietDonHangDtos.OrderByDescending(dto => dto.NgayHetHan).ToList();
            }



            // Export to Excel
            if (baoHanhSearch.isexport == 1)
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("BaoHanh");

                // Set title row
                worksheet.Cells[1, 1].Value = "Thông Tin Bảo Hành";
                worksheet.Cells[1, 1, 1, 12].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 17;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // Set header row
                worksheet.Cells[2, 1].Value = "STT";
                worksheet.Cells[2, 2].Value = "Mã đơn DV";
                worksheet.Cells[2, 3].Value = "Mã CT đơn DV";
                worksheet.Cells[2, 4].Value = "Số ĐT";
                worksheet.Cells[2, 5].Value = "Mã lỗi/LK";
                worksheet.Cells[2, 6].Value = "Tên lỗi/LK";
                worksheet.Cells[2, 7].Value = "Loại DV";
                worksheet.Cells[2, 8].Value = "Khách hàng";
                worksheet.Cells[2, 9].Value = "Ngày kích hoạt";
                worksheet.Cells[2, 10].Value = "Ngày hết hạn";
                worksheet.Cells[2, 11].Value = "Trạng thái BH";
                worksheet.Cells[2, 12].Value = "Điều kiện";

                using (var range = worksheet.Cells[2, 1, 2, 12])
                {
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    range.Style.Font.Bold = true;
                }

                // Populate data rows
                for (int i = 0; i < chiTietDonHangDtos.Count; i++)
                {
                    var record = chiTietDonHangDtos[i];
                    worksheet.Cells[i + 3, 1].Value = i + 1;
                    worksheet.Cells[i + 3, 2].Value = record.idDonDichVu ?? "N/A";
                    worksheet.Cells[i + 3, 3].Value = record.IDChiTietDonDichVu ?? "N/A";
                    worksheet.Cells[i + 3, 4].Value = record.SDT ?? "N/A";
                    worksheet.Cells[i + 3, 5].Value = record.MaLinhKien ?? record.MaLoi ?? "N/A";
                    worksheet.Cells[i + 3, 6].Value = record.TenLinhKien ?? record.TenLoi ?? "N/A";
                    worksheet.Cells[i + 3, 7].Value = record.LoaiDichVu ?? "N/A";
                    worksheet.Cells[i + 3, 8].Value = record.TenKhachHang ?? "N/A";
                    worksheet.Cells[i + 3, 9].Value = record.NgayKichHoat?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
                    worksheet.Cells[i + 3, 10].Value = record.NgayHetHan?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
                    worksheet.Cells[i + 3, 11].Value = record.TrangThaiBaoHanh ? "Còn bảo hành" : "Hết bảo hành";
                    worksheet.Cells[i + 3, 12].Value = record.DieuKien ?? "N/A";
                }

                // Footer
                int footerRow = chiTietDonHangDtos.Count + 3;
                worksheet.Cells[footerRow, 1].Value = $"Ngày xuất file: {DateTime.Now:dd/MM/yyyy HH:mm}";
                worksheet.Cells[footerRow, 1, footerRow, 3].Merge = true;
                worksheet.Cells[footerRow, 5].Value = $"Tổng số lượng: {chiTietDonHangDtos.Count}";
                worksheet.Cells[footerRow, 5, footerRow, 7].Merge = true;
                worksheet.Cells[footerRow, 1].Style.Font.Bold = true;
                worksheet.Cells[footerRow, 5].Style.Font.Bold = true;

                worksheet.Cells.AutoFitColumns();

                // Generate filename with current date
                string fileName = $"BaoHanh_{DateTime.Now:yyyyMMdd}.xlsx";
                var stream = new MemoryStream(package.GetAsByteArray());
                ViewBag.IsExport = 0;
                ViewBag.IsBaoCao = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }


            // xuất báo cáo
            // Export to Report
            if (baoHanhSearch.isBaoCao == 1)
            {
                var doc = new PdfDocument();
                var page = doc.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;
                var gfx = XGraphics.FromPdfPage(page);
                var tf = new XTextFormatter(gfx);

                // Fonts
                var font = new XFont("Arial", 9, XFontStyle.Regular);
                var fontBold = new XFont("Arial", 10, XFontStyle.Bold);
                var titleFont = new XFont("Arial", 14, XFontStyle.Bold);
                var headerFont = new XFont("Arial", 11, XFontStyle.Bold);
                var pen = new XPen(XColors.Black, 0.5);

                // Layout
                double margin = 40;
                double y = margin;
                double rowHeight = 22;
                double pageWidth = page.Width - 2 * margin;
                double pageHeight = page.Height;
                double bottomMargin = 60;

                // Method to create a new page
                PdfPage CreateNewPage(PdfDocument document, out XGraphics graphics, out XTextFormatter textFormatter)
                {
                    var newPage = document.AddPage();
                    newPage.Size = PdfSharpCore.PageSize.A4;
                    graphics = XGraphics.FromPdfPage(newPage);
                    textFormatter = new XTextFormatter(graphics);
                    return newPage;
                }

                // Header
                gfx.DrawString("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM", headerFont, XBrushes.Black, new XRect(0, y, page.Width, 15), XStringFormats.TopCenter);
                y += 15;
                gfx.DrawString("Độc lập - Tự do - Hạnh phúc", font, XBrushes.Black, new XRect(0, y, page.Width, 15), XStringFormats.TopCenter);
                y += 35;

                // Report Title
                gfx.DrawString("ĐIỆN MÁY XANH", headerFont, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("Địa chỉ: 222 Nguyễn Văn Linh, Phường Thạc Gián, Quận Thanh Khê, Tp Đà Nẵng", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("Thời gian hoạt động: 7 giờ 30 phút - 22 giờ (kể cả CN và ngày lễ)", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("Số điện thoại: 1900232461", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("BÁO CÁO BẢO HÀNH", titleFont, XBrushes.DarkRed, new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);
                y += 30;

                // Report Info
                gfx.DrawString("Họ và tên: .........................................................", font, XBrushes.Black, new XPoint(margin, y));
                gfx.DrawString("Chức vụ: ................................", font, XBrushes.Black, new XPoint(page.Width / 2 + 20, y));
                y += 15;
                gfx.DrawString($"Số điện thoại khách hàng: {(baoHanhSearch.SoDienThoaiKhachHang ?? "...")}", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString($"Từ ngày: {baoHanhSearch.TuNgay?.ToString("dd/MM/yyyy") ?? "..."} đến ngày: {baoHanhSearch.DenNgay?.ToString("dd/MM/yyyy") ?? "..."}", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString($"Tình trạng bảo hành: {(baoHanhSearch.TrangThai == null ? "Tất cả" : (baoHanhSearch.TrangThai == true ? "Còn bảo hành" : "Hết bảo hành"))}", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString("Bộ phận công tác: .........................................................", font, XBrushes.Black, new XPoint(margin, y));
                y += 15;
                gfx.DrawString($"Thời gian thực hiện: Ngày {DateTime.Now:dd} Tháng {DateTime.Now:MM} Năm {DateTime.Now:yyyy}", font, XBrushes.Black, new XPoint(margin, y));
                y += 25;

                // Table Title
                gfx.DrawString("I. DANH SÁCH BẢO HÀNH", fontBold, XBrushes.Black, new XPoint(margin, y));
                y += 20;

                // Table Headers
                double[] colWidths = { 30, 40, 40, 60, 50, 60, 50, 60, 50, 50, 50 };
                string[] headers = { "STT", "Mã đơn DV", "Mã CT đơn DV", "Số ĐT", "Mã lỗi/LK", "Tên lỗi/LK", "Loại DV", "Khách hàng", "Ngày kích hoạt", "Ngày hết hạn", "Trạng thái BH" };
                double colX = margin;

                for (int i = 0; i < headers.Length; i++)
                {
                    gfx.DrawRectangle(pen, XBrushes.LightGray, colX, y, colWidths[i], rowHeight);
                    //tf.DrawString(headers[i], fontBold, XBrushes.Black, new XRect(colX + 3, y + 5, colWidths[i] - 6, rowHeight), XStringFormats.TopLeft);
                    var rect = new XRect(colX + 3, y + 3, colWidths[i] - 6, rowHeight - 6);
                    tf.DrawString(headers[i], font, XBrushes.Black, rect);
                    colX += colWidths[i];
                }
                y += rowHeight;

                // Table Data
                int maxRows = chiTietDonHangDtos.Count;
                for (int i = 0; i < maxRows; i++)
                {
                    var item = chiTietDonHangDtos[i];
                    string[] values = {
                (i + 1).ToString(),
                item.idDonDichVu ?? "N/A",
                item.IDChiTietDonDichVu ?? "N/A",
                item.SDT ?? "N/A",
                item.MaLinhKien ?? item.MaLoi ?? "N/A",
                item.TenLinhKien ?? item.TenLoi ?? "N/A",
                item.LoaiDichVu ?? "N/A",
                item.TenKhachHang ?? "N/A",
                item.NgayKichHoat?.ToString("dd/MM/yyyy") ?? "N/A",
                item.NgayHetHan?.ToString("dd/MM/yyyy") ?? "N/A",
                item.TrangThaiBaoHanh ? "Còn bảo hành" : "Hết bảo hành"
            };

                    // Estimate row height
                    double rowMaxHeight = rowHeight;
                    for (int j = 0; j < values.Length; j++)
                    {
                        string text = string.IsNullOrEmpty(values[j]) ? "N/A" : values[j];
                        var size = gfx.MeasureString(text, font);
                        int lineCount = (int)Math.Ceiling(size.Width / (colWidths[j] - 6));
                        rowMaxHeight = Math.Max(rowMaxHeight, rowHeight * Math.Min(3, lineCount));
                    }

                    // If not enough space on the current page, create a new page
                    if (y + rowMaxHeight + bottomMargin > pageHeight)
                    {
                        page = CreateNewPage(doc, out gfx, out tf);
                        y = margin;

                        // Redraw table headers
                        colX = margin;
                        for (int j = 0; j < headers.Length; j++)
                        {
                            gfx.DrawRectangle(pen, XBrushes.LightGray, colX, y, colWidths[j], rowHeight);
                            //tf.DrawString(headers[j], fontBold, XBrushes.Black, new XRect(colX + 3, y + 5, colWidths[j] - 6, rowHeight), XStringFormats.TopLeft);
                            var rect = new XRect(colX + 3, y + 3, colWidths[j] - 6, rowMaxHeight - 6);
                            tf.DrawString(values[j], font, XBrushes.Black, rect);
                            colX += colWidths[j];
                        }
                        y += rowHeight;
                    }

                    // Draw data row
                    colX = margin;
                    for (int j = 0; j < values.Length; j++)
                    {
                        string text = string.IsNullOrEmpty(values[j]) ? "N/A" : values[j];
                        gfx.DrawRectangle(pen, colX, y, colWidths[j], rowMaxHeight);
                        //tf.DrawString(text, font, XBrushes.Black, new XRect(colX + 3, y + 3, colWidths[j] - 6, rowMaxHeight - 6), XStringFormats.TopLeft);
                        var rect = new XRect(colX + 3, y + 3, colWidths[j] - 6, rowMaxHeight - 6);
                        tf.DrawString(values[j], font, XBrushes.Black, rect);
                        colX += colWidths[j];
                    }
                    y += rowMaxHeight;
                }

                // Footer
                y += 20;
                gfx.DrawString($"Tổng số lượng đơn bảo hành: {chiTietDonHangDtos.Count}", fontBold, XBrushes.Black, new XPoint(margin, y));
                y += 20;
                gfx.DrawString("Ghi chú: Báo cáo có hiệu lực trong vòng 7 ngày kể từ ngày xuất báo cáo.", font, XBrushes.Black, new XPoint(margin, y));
                y += 35;
                gfx.DrawString("PHỤ TRÁCH BỘ PHẬN", fontBold, XBrushes.Black, new XPoint(margin + 30, y));
                gfx.DrawString("NGƯỜI BÁO CÁO", fontBold, XBrushes.Black, new XPoint(page.Width - margin - 130, y));

                // Save PDF
                using var stream = new MemoryStream();
                doc.Save(stream, false);
                string fileName = $"BaoCaoBaoHanh_{DateTime.Now:yyyyMMddHHmm}.pdf";
                ViewBag.IsExport = 0;
                ViewBag.IsBaoCao = 0;
                return File(stream.ToArray(), "application/pdf", fileName);
            }



            int TotalRecords = chiTietDonHangDtos.Count;
            int TotalPage= (int)Math.Ceiling((double)TotalRecords / pageSize);
            chiTietDonHangDtos = chiTietDonHangDtos.Skip((pageIndex - 1) * pageSize)
            .Take(pageSize).ToList();
            paging.TotalPage = TotalPage;
            paging.PageActive = baoHanhSearch.PageActive;

            // Fetch additional data for the view
            var loaiLK = await _context.LoaiLinhKiens.ToListAsync();
            var NhaSX = await _context.NhaSanXuats.ToListAsync();
            BaoHanhSearch baoHanhSearchNew = new BaoHanhSearch();
            baoHanhSearchNew = baoHanhSearch;
            BaoHanhSearchVM baohanhView = new BaoHanhSearchVM
            {
                ChiTietDonHangs = chiTietDonHangDtos, 
                nhaSanXuats = NhaSX,
                linhKiens = loaiLK,
                Paging = paging,
                baoHanhSearch =baoHanhSearchNew 
            };

            return View(baohanhView);
        }
    }
}
