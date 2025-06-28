namespace QLSuaChuaVaLapDat.Models.TimKiem
{
    public class BaoHanhSearch
    {
        public string MaDonDichVu { get; set; }
        public string MaLinhKien { get; set; }
        public string SoDienThoaiKhachHang { get; set; }
        public bool? TrangThai { get; set; } = null;
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public string? LoaiLinhKien { get; set; } = null;
        public string? NhaSanXuat { get; set; }
        public int PageActive { get; set; } = 1;


        public string SapXepTheoidDonDichVu { get; set; }
        public string SapXepTheoIDChiTietDonDichVu { get; set; }
        public string SapXepTheoMaLinhKien { get; set; }
        public string SapXepTheoTenLinhKien { get; set; }
        public string SapXepTheoTenKhachHang { get; set; }
        public string SapXepTheoNgayKichHoat { get; set; }
        public string SapXepTheoNgayHetHan { get; set; }
        public string SapXepTheoTrangThaiBaoHanh { get; set; }
        public int? isexport { get; set; } = 0;

        public int? isBaoCao { get; set; } = 0;
    }
}
