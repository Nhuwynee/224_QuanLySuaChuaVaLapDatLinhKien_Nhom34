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
        public string SapXepTheoIdDonDichVu { get; set; }

        public string SapXepTheoTenKhachHang { set; get; }

        public string SapXepTheoTongTien { set; get; }

        public string SapXepTheoNgayTao { set; get; }


        public int? isexport { get; set; } = 0;

        public int? isBaoCao { get; set; } = 0;

        public int PageActive { get; set; } = 1;
        public DonDichVuSearch()
        {
            TrangThaiDV = null;
            IdLoaiThietBi = null;
            LoaiDichVu = null;
        }

    }
}
