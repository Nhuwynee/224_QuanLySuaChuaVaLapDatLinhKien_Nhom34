namespace QLSuaChuaVaLapDat.Models.TimKiem
{
    public class KhachHangSearch
    {
        public string TenKhachHang { get; set; } 
        public string SoDienThoai { get; set; } 
        public string Email { get; set; }       
        public string LoaiKhachHang { get; set; }
        public string ThanhPho { get; set; }   
        public string QuanHuyen { get; set; }   
        public string PhuongXa { get; set; }    
        public string NhomKhachHang { get; set; } 
        public string CuaHang { get; set; }
        public string? SapXepTheoMaKH { get; set; } = null;
        public string? SapXepTheoTenUser { get; set; } = null;
        public string? SapXepTheoTenKhachHang { get; set; } = null;
        public string? SapXepTheoTongSoDon { get; set; } = null;
        public string? SapXepTheoTongSoTienSuaChua { get; set; } = null;
        public int pageActive { get; set; }

        public int? isexport { get; set; } = 0;

        public int? isBaoCao { get; set; } = 0;

        public KhachHangSearch() {
            NhomKhachHang = null;
           
        }
    }
}
