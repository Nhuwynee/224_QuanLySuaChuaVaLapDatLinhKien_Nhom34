namespace QLSuaChuaVaLapDat.Models.TimKiem
{
    public class LinhKienSearch
    {
        public string MaLinhKien { get; set; } 
        public string TenLinhKien { get; set; } 
        public string LoaiLinhKien { get; set; } 
        public string NhaSanXuat { get; set; }
        public decimal? GiaTu { get; set; }
        public decimal? GiaDen { get; set; } 
        public int? BaoHanh { get; set; } 
        public string LocTheo { get; set; } 
        public string SapXep { get; set; } 
        public int TTSanPham { set; get; }
        public int PageActive { get; set; } = 1;
        public string SapXepTheoMaLinhKien { get; set; }
        public string SapXepTheoTenLinhKien { get; set; }
        public string SapXepTheoLoaiLinhKien { get; set; }
        public string SapXepTheoNhaSanXuat { get; set; }
        public string SapXepTheoGia { get; set; }
        public string SapXepTheoSoLuong { get; set; }
        public string SapXepTheoBaoHanh { get; set; }
        public int? isexport { get; set; } = 0;

        public int? isBaoCao { get; set; } = 0;
        public LinhKienSearch()
        {
            LoaiLinhKien = "Tất cả";
            NhaSanXuat = "Tất cả";
            TTSanPham = 1;
            BaoHanh = null;
            LocTheo = "Tình trạng"; 
            SapXep = "Tên A-Z"; 
        }
    }
}