namespace QLSuaChuaVaLapDat.Models.TimKiem
{
    public class DonDichVuSearch
    {
        public string MaDonDichVu { get; set; }   
        public string SDTKhachHang { get; set; }           
        public string IDKyThuatVien { get; set; }    
        public string? TrangThaiDV { get; set; }     
        public string TuNgay { get; set; }       
        public string DenNgay { get; set; }       
        public string? LoaiDichVu { get; set; }          
        public string? LoaiThiếtBi { get; set; }
        public string? SapXepTheo { get; set; }

        public DonDichVuSearch()
        {
            TrangThaiDV = null;
            LoaiThiếtBi = null;
            LoaiDichVu = null;
        }
    }
}
