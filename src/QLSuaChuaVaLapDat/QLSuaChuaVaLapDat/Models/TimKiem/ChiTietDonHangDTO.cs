namespace QLSuaChuaVaLapDat.Models.TimKiem
{
    public class ChiTietDonHangDTO
    {
        public string idDonDichVu {  get; set; }
        public string IDChiTietDonDichVu { get; set; } 
        public string SDT { set; get; }
        public string MaLinhKien { get; set; } 
        public string MaLoi { get; set; }
        public string TenLinhKien { get; set; } 
        public string TenLoi { get; set; } 
        public string LoaiDichVu { get; set; } 
        public string TenKhachHang { get; set; } 
        public DateTime? NgayKichHoat { get; set; } 
        public DateTime? NgayHetHan { get; set; } 
        public bool TrangThaiBaoHanh { get; set; } 
        public string DieuKien { get; set; } 
    }
}
