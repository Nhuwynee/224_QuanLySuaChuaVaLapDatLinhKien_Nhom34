namespace QLSuaChuaVaLapDat.Models.TimKiem
{
    public class DonDichVuSearch
    {
        public string MaDonDichVu { get; set; } = null;   
        public string TenKhachHang { get; set; } = null;
        public string IDKyThuatVien { get; set; } = null;
        public string? TrangThaiDV { get; set; } = null;
        public string TuNgay { get; set; } = null;
        public string DenNgay { get; set; } = null;
        public string? LoaiDichVu { get; set; } = null;
        public string? IdLoaiThietBi { get; set; } = null;
        public string? SapXepTheo { get; set; } = null;

        public int PageActive { get; set; } = 1;
        public DonDichVuSearch()
        {
            TrangThaiDV = null;
            IdLoaiThietBi = null;
            LoaiDichVu = null;
        }

    }
}
