namespace QLSuaChuaVaLapDat.Models.TimKiem
{
    public class BaoHanhSearch
    {
        public string MaDonDichVu { get; set; }
        public string MaLinhKien { get; set; }
        public string SoDienThoaiKhachHang { get; set; }
        public string? TrangThai { get; set; } = null;
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public string? LoaiLinhKien { get; set; } = null;
        public string? NhaSanXuat { get; set; }
        public string SapXepTheo { get; set; }
    }
}
