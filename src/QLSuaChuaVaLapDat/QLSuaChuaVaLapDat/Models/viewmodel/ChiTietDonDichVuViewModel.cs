namespace QLSuaChuaVaLapDat.Models.viewmodel
{
    public class ChiTietDonDichVuViewModel
    {
        public string MaDon { get; set; }
        public string TenKhachHang { get; set; }
        public string LoaiKhachHang { get; set; }
        public DateTime? NgayTaoDon { get; set; }
        public DateTime? NgayHoanThanh { get; set; }
        public string TenNhanVienKyThuat { get; set; }
        public string TenNguoiTaoDon { get; set; }
        public string TenThietBi { get; set; }
        public string LoaiThietBi { get; set; }
        public string HinhThucDichVu { get; set; }
        public string LoaiDonDichVu { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThaiDon { get; set; }
        public DateTime? NgayChinhSua { get; set; }
        
        // Thông tin lỗi - Giữ lại để tương thích với code cũ
        public string TenLinhKien { get; set; }
        public string MoTaLoi { get; set; }
        public string MoTaChiTiet { get; set; }
        public decimal DonGiaLoi { get; set; }
        
        // Danh sách chi tiết lỗi
        public List<ChiTietLoiViewModel> DanhSachChiTietLoi { get; set; } = new List<ChiTietLoiViewModel>();
    }
}
