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