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
        public string SapXepTheo { get; set; }   
        
        public KhachHangSearch() {
            NhomKhachHang = null;
            SapXepTheo = null;
        }
    }
}
