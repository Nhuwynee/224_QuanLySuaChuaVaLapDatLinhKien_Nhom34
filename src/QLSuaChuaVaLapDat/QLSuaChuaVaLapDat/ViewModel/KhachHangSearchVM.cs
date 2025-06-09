using QLSuaChuaVaLapDat.Models;
using QLSuaChuaVaLapDat.Models.TimKiem;

namespace QLSuaChuaVaLapDat.ViewModel
{
    public class KhachHangSearchVM
    {
        public KhachHang KhachHangs { get; set; }
        public List<ThanhPhoDTO> ThanhPhos {  get; set; }
        public List<QuanDTO> Quans { get; set; }
        public Paging Paging { get; set; }
        public List<PhuongDTO> Phuongs { get; set; }
    }
}
