namespace QLSuaChuaVaLapDat.Models.viewmodel
{
    public class ChiTietLoiViewModel
    {
        public string IdChiTiet { get; set; }
        public string TenLinhKien { get; set; }
        public string MoTaLoi { get; set; }
        public string MoTaChiTiet { get; set; }
        public decimal DonGiaLoi { get; set; }
        public int SoLuong { get; set; }
        public string LoaiDichVu { get; set; }
        public DateTime? NgayKetThucBh { get; set; }
        public DateTime? ThoiGianThemLinhKien { get; set; }
        public bool? HanBaoHanh { get; set; }
        // Thêm dòng sau
        public List<string> DanhSachAnh { get; set; }
    }
}
