using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSuaChuaVaLapDat.Models;

namespace QLSuaChuaVaLapDat.Controllers.DanhSachDonDichVuController
{
    public class DanhSachDonDichVuController : Controller
    {
        private readonly QuanLySuaChuaVaLapDatContext _context;
        public DanhSachDonDichVuController(QuanLySuaChuaVaLapDatContext context)
        {
            _context = context;
        }
        //public IActionResult IndexDSDDV()
        //{
        //    var danhSach = (from don in _context.DonDichVus
        //        join ct in _context.ChiTietDonDichVus on don.IdDonDichVu equals ct.IdDonDichVu
        //        join u in _context.Users on don.IdUser equals u.IdUser into userJoin
        //        from u in userJoin.DefaultIfEmpty()
        //        join kvl in _context.KhachVangLais on don.IdKhachVangLai equals kvl.IdKhachVangLai into kvlJoin
        //        from kvl in kvlJoin.DefaultIfEmpty()
        //        group new { don, ct, u, kvl } by don.IdDonDichVu into g
        //        select new DonDichVuViewModel
        //        {
        //            MaDon = g.Key,
        //            TenKhachHang = g.FirstOrDefault().don.IdUser != null ?
        //                g.FirstOrDefault().u.HoVaTen :
        //                g.FirstOrDefault().kvl.HoVaTen,
        //            TrangThai = g.FirstOrDefault().don.TrangThaiDon,
        //            TongTien = g.FirstOrDefault().don.TongTien ?? 0,
        //            MoTa = string.Join(", ", g.Select(x => x.ct.MoTa))
        //        }).ToList();

        //    return View(danhSach);
        //}
        public IActionResult IndexDSDDV(int page = 1)
        {
            int pageSize = 5;

            var query = from don in _context.DonDichVus
                join ct in _context.ChiTietDonDichVus on don.IdDonDichVu equals ct.IdDonDichVu
                join u in _context.Users on don.IdUser equals u.IdUser into userJoin
                from u in userJoin.DefaultIfEmpty()
                join kvl in _context.KhachVangLais on don.IdKhachVangLai equals kvl.IdKhachVangLai into kvlJoin
                from kvl in kvlJoin.DefaultIfEmpty()
                group new { don, ct, u, kvl } by don.IdDonDichVu into g
                select new DonDichVuViewModel
                {
                    MaDon = g.Key,
                    TenKhachHang = g.FirstOrDefault().don.IdUser != null ?
                        g.FirstOrDefault().u.HoVaTen :
                        g.FirstOrDefault().kvl.HoVaTen,
                    TrangThai = g.FirstOrDefault().don.TrangThaiDon,
                    TongTien = g.FirstOrDefault().don.TongTien ?? 0,
                    MoTa = string.Join(", ", g.Select(x => x.ct.MoTa))
                };

            int totalCount = query.Count();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var danhSach = query
                .OrderBy(x => x.MaDon)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new DonDichVuPagedViewModel
            {
                DonDichVus = danhSach,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View("IndexDSDDV",viewModel);
        }


    }
}
