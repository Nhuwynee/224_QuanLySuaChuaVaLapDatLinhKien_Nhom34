using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QLSuaChuaVaLapDat.Models;
using QLSuaChuaVaLapDat.Models.Impl;
using QLSuaChuaVaLapDat.Models.TimKiem;
using QLSuaChuaVaLapDat.ViewModel;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;


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
                string trangThai = donDichVuSearch.TrangThaiDV switch
                {
                    "processing" => "Đang sửa chữa",
                    "completed" => "Hoàn thành",
                    _ => donDichVuSearch.TrangThaiDV
                };
                query = query.Where(d => d.TrangThaiDon == trangThai);
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

            

            if (!string.IsNullOrEmpty(donDichVuSearch.SapXepTheo))
            {
                query = donDichVuSearch.SapXepTheo switch
                {
                    "NgayTaoDesc" => query.OrderByDescending(d => d.NgayTaoDon),
                    "NgayTaoAsc" => query.OrderBy(d => d.NgayTaoDon),
                    "TongTienDesc" => query.OrderByDescending(d => d.TongTien),
                    "TongTienAsc" => query.OrderBy(d => d.TongTien),
                    _ => query.OrderByDescending(d => d.NgayTaoDon),
                };
            }
            else
            {
                query = query.OrderByDescending(d => d.NgayTaoDon);
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
            int pageUser = 1;
            int pageKVL = 1;
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

            int totalUserRecords = await queryUsers.CountAsync();
            int totalUserPages = (int)Math.Ceiling((double)totalUserRecords / pageSize);

            var dsKH = await queryUsers
                .OrderBy(u => u.User.IdUser) // Sắp xếp theo ý muốn
                .Skip((pageUser - 1) * pageSize)
                .Take(pageSize)
                .Select(result => new NguoiDung
                {
                    IdUser = result.User.IdUser,
                    TenUser = result.User.TenUser,
                    HoVaTen = result.User.HoVaTen,
                    Sdt = result.User.Sdt,
                    NgaySinh = result.User.NgaySinh,
                    TrangThai = result.User.TrangThai,
                    DiaChi = (result.User.DiaChi ?? "") +
                             (result.User.IdPhuongNavigation != null ? ", " + result.User.IdPhuongNavigation.TenPhuong : "") +
                             (result.User.IdPhuongNavigation != null && result.User.IdPhuongNavigation.IdQuanNavigation != null
                                 ? ", " + result.User.IdPhuongNavigation.IdQuanNavigation.TenQuan : "") +
                             (result.User.IdPhuongNavigation != null && result.User.IdPhuongNavigation.IdQuanNavigation != null
                                 && result.User.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation != null
                                 ? ", " + result.User.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation.TenThanhPho : ""),
                    tongDonHang = (double)result.TongGiaTri,
                    TongDon = result.TongDon
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

            int totalKVLRecords = await queryKVL.CountAsync();
            int totalKVLPages = (int)Math.Ceiling((double)totalKVLRecords / (pageSize*2));

            var dsKVL = await queryKVL
                .OrderBy(k => k.KhachVangLai.IdKhachVangLai) // Sắp xếp theo ý muốn
                .Skip((pageKVL - 1) * pageSize)
                .Take(pageSize)
                .Select(result => new KhachVangLaiImpl
                {
                    IdKhachVangLai = result.KhachVangLai.IdKhachVangLai,
                    HoVaTen = result.KhachVangLai.HoVaTen,
                    Sdt = result.KhachVangLai.Sdt,
                    DiaChi = (result.KhachVangLai.DiaChi ?? "") +
                             (result.KhachVangLai.IdPhuongNavigation != null ? ", " + result.KhachVangLai.IdPhuongNavigation.TenPhuong : "") +
                             (result.KhachVangLai.IdPhuongNavigation != null && result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation != null
                                 ? ", " + result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation.TenQuan : "") +
                             (result.KhachVangLai.IdPhuongNavigation != null && result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation != null
                                 && result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation != null
                                 ? ", " + result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation.TenThanhPho : ""),
                    IdPhuong = result.KhachVangLai.IdPhuong,
                    TongDonHang = (double)result.TongGiaTri
                }).ToListAsync();

            // Lấy danh sách địa phương (thành phố, quận, phường)
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
            KhachHang dsKhachHang = new KhachHang();
            dsKhachHang.KhachVangLais = dsKVL;
            dsKhachHang.Users = dsKH;

            KhachHangSearchVM viewKH = new KhachHangSearchVM();
            viewKH.Phuongs = Phuongs;
            viewKH.Quans = Quans;
            viewKH.ThanhPhos = ThanhPhos;
            viewKH.KhachHangs = dsKhachHang;

            paging.TotalPage = totalUserPages + totalKVLPages;
            viewKH.Paging = paging;
            return View(viewKH);
        }


        [HttpPost]
        public async Task<IActionResult> TimKiemKhachHang(KhachHangSearch khachHangSearch)
        {
            Paging paging = new Paging();
            int pageUser = khachHangSearch.pageActive ==null? 1 : khachHangSearch.pageActive;

            int pageSize = paging.PageSize;
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

            if (!string.IsNullOrEmpty(khachHangSearch.LoaiKhachHang))
            {
                if (khachHangSearch.LoaiKhachHang == "Users")
                {
                    khachVangLaiQuery = khachVangLaiQuery.Where(k => false);
                }
                else if (khachHangSearch.LoaiKhachHang == "KhachVangLais")
                {
                    userQuery = userQuery.Where(u => false);
                }
            }

            // Sorting
            if (!string.IsNullOrEmpty(khachHangSearch.SapXepTheo))
            {
                switch (khachHangSearch.SapXepTheo)
                {
                    case "asc":
                        userQuery = userQuery.OrderBy(u => u.User.HoVaTen);
                        khachVangLaiQuery = khachVangLaiQuery.OrderBy(k => k.KhachVangLai.HoVaTen);
                        break;
                    case "desc":
                        userQuery = userQuery.OrderByDescending(u => u.User.HoVaTen);
                        khachVangLaiQuery = khachVangLaiQuery.OrderByDescending(k => k.KhachVangLai.HoVaTen);
                        break;
                    case "ddvdesc":
                        userQuery = userQuery.OrderByDescending(u => u.TongDon);
                        break;
                    case "ddvasc":
                        userQuery = userQuery.OrderBy(u => u.TongDon);
                        break;
                    case "ctasc":
                        userQuery = userQuery.OrderBy(u => u.TongGiaTri);
                        khachVangLaiQuery = khachVangLaiQuery.OrderBy(k => k.TongGiaTri);
                        break;
                    case "ctdesc":
                        userQuery = userQuery.OrderByDescending(u => u.TongGiaTri);
                        khachVangLaiQuery = khachVangLaiQuery.OrderByDescending(k => k.TongGiaTri);
                        break;
                    default:
                        userQuery = userQuery.OrderBy(u => u.User.HoVaTen);
                        khachVangLaiQuery = khachVangLaiQuery.OrderBy(k => k.KhachVangLai.HoVaTen);
                        break;
                }
            }
            else
            {
                userQuery = userQuery.OrderBy(u => u.User.HoVaTen);
                khachVangLaiQuery = khachVangLaiQuery.OrderBy(k => k.KhachVangLai.HoVaTen);
            }

            // Execute queries
            var dsKH = await userQuery
                .Select(result => new NguoiDung
                {
                    IdUser = result.User.IdUser,
                    TenUser = result.User.TenUser,
                    HoVaTen = result.User.HoVaTen,
                    Sdt = result.User.Sdt,
                    NgaySinh = result.User.NgaySinh,
                    TrangThai = result.User.TrangThai,
                    GioiTinh = result.User.GioiTinh,
                    DiaChi = result.User.DiaChi +
                             (result.User.IdPhuongNavigation != null ? ", " + result.User.IdPhuongNavigation.TenPhuong : "") +
                             (result.User.IdPhuongNavigation != null && result.User.IdPhuongNavigation.IdQuanNavigation != null
                                 ? ", " + result.User.IdPhuongNavigation.IdQuanNavigation.TenQuan : "") +
                             (result.User.IdPhuongNavigation != null && result.User.IdPhuongNavigation.IdQuanNavigation != null
                                 && result.User.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation != null
                                 ? ", " + result.User.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation.TenThanhPho : ""),
                    tongDonHang = (double)result.TongGiaTri,
                    TongDon = result.TongDon
                })
                .ToListAsync();

                var dsKVL = await khachVangLaiQuery
                    .Select(result => new KhachVangLaiImpl
                    {
                        IdKhachVangLai = result.KhachVangLai.IdKhachVangLai,
                        HoVaTen = result.KhachVangLai.HoVaTen,
                        Sdt = result.KhachVangLai.Sdt,
                        DiaChi = result.KhachVangLai.DiaChi +
                                 (result.KhachVangLai.IdPhuongNavigation != null ? ", " + result.KhachVangLai.IdPhuongNavigation.TenPhuong : "") +
                                 (result.KhachVangLai.IdPhuongNavigation != null && result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation != null
                                     ? ", " + result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation.TenQuan : "") +
                                 (result.KhachVangLai.IdPhuongNavigation != null && result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation != null
                                     && result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation != null
                                     ? ", " + result.KhachVangLai.IdPhuongNavigation.IdQuanNavigation.IdThanhPhoNavigation.TenThanhPho : ""),
                        IdPhuong = result.KhachVangLai.IdPhuong,
                        TongDonHang = (double)result.TongGiaTri
                    })
                    .ToListAsync();

                    if (!string.IsNullOrEmpty(khachHangSearch.TenKhachHang))
                    {
                        string keyword = RemoveDiacritics(khachHangSearch.TenKhachHang.ToLower());
                        dsKH = dsKH.Where(u => RemoveDiacritics(u.HoVaTen.ToLower()).Contains(keyword)).ToList();
                        dsKVL = dsKVL.Where(k => RemoveDiacritics(k.HoVaTen.ToLower()).Contains(keyword)).ToList();
                    }

            int totalKH =  dsKH.Count;
            int totalKVL = dsKVL.Count;
            int totalRecords = totalKH + totalKVL;
            int totalPage = 0;
            if (totalKH!=0 && totalKVL != 0)
            {
                totalPage = (int)Math.Ceiling((double)totalRecords / (pageSize*2));
            }
            totalPage = (int)Math.Ceiling((double)totalRecords / (pageSize));

            List<NguoiDung> dsNguoiDung = dsKH
                .Skip((pageUser - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            List<KhachVangLaiImpl> dsKhachVL = dsKVL
               .Skip((pageUser - 1) * pageSize)
               .Take(pageSize)
               .ToList();

            paging.TotalPage = totalPage;
            paging.PageActive = khachHangSearch.pageActive;

            // Populate DTOs for dropdowns
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
            KhachHangSearch khSearch = new KhachHangSearch();
            khSearch = khachHangSearch;

            // Prepare ViewModel
            var khachHang = new KhachHang
            {
                Users = dsNguoiDung,
                KhachVangLais = dsKhachVL 
            };

            var viewKH = new KhachHangSearchVM
            {
                Phuongs = phuongs,
                Quans = quans,
                ThanhPhos = thanhPhos,
                KhachHangs = khachHang,
                Paging = paging,
                KhachHangSearch = khSearch
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
                    query = query.Where(l => l.SoLuong >= 1);
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
                TenKhachHang = ct.IdDonDichVuNavigation.IdKhachVangLaiNavigation?.HoVaTen ?? ct.IdDonDichVuNavigation.IdUserNavigation?.TenUser ?? "Khách vãng lai",
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
                query = query.Where(ct => ct.IdDonDichVuNavigation.TrangThaiDon == baoHanhSearch.TrangThai);
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
            if (!string.IsNullOrEmpty(baoHanhSearch.SapXepTheo))
            {
                switch (baoHanhSearch.SapXepTheo)
                {
                    case "datedesc":
                        chiTietDonHangDtos = chiTietDonHangDtos.OrderByDescending(dto => dto.NgayKichHoat).ToList();
                        break;
                    case "dateasc":
                        chiTietDonHangDtos = chiTietDonHangDtos.OrderBy(dto => dto.NgayKichHoat).ToList();
                        break;
                    default:
                        chiTietDonHangDtos = chiTietDonHangDtos.OrderBy(dto => dto.idDonDichVu).ToList();
                        break;
                }
            }
            else
            {
                chiTietDonHangDtos = chiTietDonHangDtos.OrderBy(dto => dto.idDonDichVu).ToList();
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
