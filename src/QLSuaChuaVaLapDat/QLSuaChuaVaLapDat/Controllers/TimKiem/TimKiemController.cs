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

        [HttpGet]
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

        [HttpPost]
        public async Task<IActionResult> TimKiemDonDichVu(DonDichVuSearch donDichVuSearch)
        {
            var query = _context.DonDichVus
                .Include(d => d.ChiTietDonDichVus)
                .Include(d => d.IdUserTaoDonNavigation)
                .Include(d=>d.IdNhanVienKyThuatNavigation)
                .Include(d => d.IdKhachVangLaiNavigation)
                .Include(d => d.IdLoaiThietBiNavigation)
                .AsQueryable();

      
            if (!string.IsNullOrEmpty(donDichVuSearch.MaDonDichVu))
            {
                query = query.Where(d => d.IdDonDichVu.Contains(donDichVuSearch.MaDonDichVu));
            }

            if (!string.IsNullOrEmpty(donDichVuSearch.SDTKhachHang))
            {
                query = query.Where(d => (d.IdKhachVangLaiNavigation != null && d.IdKhachVangLaiNavigation.HoVaTen.Contains(donDichVuSearch.SDTKhachHang)) ||
                                         (d.IdUserTaoDonNavigation != null && d.IdUserTaoDonNavigation.HoVaTen.Contains(donDichVuSearch.SDTKhachHang)));
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
                query = query.Where(d => d.TrangThaiDon == trangThai || (trangThai == "Đang xử lý" && d.TrangThaiDon == "Đang sửa chữa"));
            }

            if (!string.IsNullOrEmpty(donDichVuSearch.TuNgay) && DateTime.TryParse(donDichVuSearch.TuNgay, out var tuNgay))
            {
                query = query.Where(d => d.NgayTaoDon >= tuNgay);
            }

            if (!string.IsNullOrEmpty(donDichVuSearch.DenNgay) && DateTime.TryParse(donDichVuSearch.DenNgay, out var denNgay))
            {
                query = query.Where(d => d.NgayTaoDon <= denNgay);
            }

            if (!string.IsNullOrEmpty(donDichVuSearch.LoaiThiếtBi))
            {
                query = query.Where(d => d.IdLoaiThietBiNavigation.TenLoaiThietBi == donDichVuSearch.LoaiThiếtBi);
            }

            if (!string.IsNullOrEmpty(donDichVuSearch.LoaiDichVu))
            {
                query = query.Where(d => d.LoaiDonDichVu == donDichVuSearch.LoaiDichVu);
            }

// Sắp xếp theo 
            if (!string.IsNullOrEmpty(donDichVuSearch.SapXepTheo))
            {
                switch (donDichVuSearch.SapXepTheo)
                {
                    case "NgayTaoDesc":
                        query = query.OrderByDescending(d => d.NgayTaoDon);
                        break;
                    case "NgayTaoAsc":
                        query = query.OrderBy(d => d.NgayTaoDon);
                        break;
                    case "TongTienDesc":
                        query = query.OrderByDescending(d => d.TongTien);
                        break;
                    case "TongTienAsc":
                        query = query.OrderBy(d => d.TongTien);
                        break;
                    default:
                        query = query.OrderByDescending(d => d.NgayTaoDon);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(d => d.NgayTaoDon); 
            }

            var result = await query.ToListAsync();
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> TimKiemKhachHang()
        {
            
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
                         TongGiaTri = orders.Sum(o => o.TongTien),
                         TongDon = orders.Count()
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
                     tongDonHang = (double)result.TongGiaTri,
                     TongDon = result.TongDon
                 }).ToListAsync();


            var dsKVL = await _context.KhachVangLais
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
                     TongDon = orders.Count() // Optional: Include total number of orders
                 })
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

            var ThanhPhos = _context.ThanhPhos
                .Select(tp => new ThanhPhoDTO
                {
                    IdThanhPho = tp.IdThanhPho,
                    TenThanhPho = tp.TenThanhPho
                })
                .ToList();
            var Quans = _context.Quans
                .Select(q => new QuanDTO
                {
                    IdQuan = q.IdQuan,
                    IdThanhPho = q.IdThanhPho,
                    TenQuan = q.TenQuan
                })
                .ToList();
            var Phuongs = _context.Phuongs
                .Select(p => new PhuongDTO
                {
                    IdPhuong = p.IdPhuong,
                    IdQuan = p.IdQuan,
                    IdThanhPho = p.IdThanhPho,
                    TenPhuong = p.TenPhuong
                })
                .ToList();


            KhachHang dsKhachHang = new KhachHang();
            dsKhachHang.KhachVangLais = dsKVL;
            dsKhachHang.Users = dsKH;
            KhachHangSearchVM viewKH = new KhachHangSearchVM();
            viewKH.Phuongs = Phuongs;
            viewKH.Quans = Quans;
            viewKH.ThanhPhos = ThanhPhos;
            viewKH.KhachHangs = dsKhachHang;
            return View(viewKH);
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> TimKiemKhachHang(KhachHangSearch khachHangSearch)
        {
            // Initialize queries for Users and KhachVangLais
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

            // Apply filters based on KhachHangSearch
            if (!string.IsNullOrEmpty(khachHangSearch.TenKhachHang))
            {
                userQuery = userQuery.Where(u => u.User.HoVaTen.Contains(khachHangSearch.TenKhachHang));
                khachVangLaiQuery = khachVangLaiQuery.Where(k => k.KhachVangLai.HoVaTen.Contains(khachHangSearch.TenKhachHang));
            }

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

            // Filter by LoaiKhachHang
            if (!string.IsNullOrEmpty(khachHangSearch.LoaiKhachHang))
            {
                if (khachHangSearch.LoaiKhachHang == "Users")
                {
                    khachVangLaiQuery = khachVangLaiQuery.Where(k => false); // Exclude KhachVangLai
                }
                else if (khachHangSearch.LoaiKhachHang == "KhachVangLais")
                {
                    userQuery = userQuery.Where(u => false); // Exclude Users
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

            // Prepare ViewModel
            var khachHang = new KhachHang
            {
                Users = dsKH,
                KhachVangLais = dsKVL // Cast to base class
            };

            var viewKH = new KhachHangSearchVM
            {
                Phuongs = phuongs,
                Quans = quans,
                ThanhPhos = thanhPhos,
                KhachHangs = khachHang
            };

            return View(viewKH);
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
        public async Task<IActionResult> TimKiemLinhKien(LinhKienSearch timKiemLinhKien)
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
