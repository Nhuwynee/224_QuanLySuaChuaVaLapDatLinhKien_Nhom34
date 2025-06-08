using QLSuaChuaVaLapDat.Models;
using QLSuaChuaVaLapDat.Models.TimKiem;

namespace QLSuaChuaVaLapDat.ViewModel
{
    public class KhachHangSearchVM
    {
        public List<KhachHangViewTimKiem> KhachHangs { get; set; }
        public List<ThanhPhoDTO> ThanhPhos {  get; set; }
        public List<QuanDTO> Quans { get; set; }
        public Paging Paging { get; set; }
        public List<PhuongDTO> Phuongs { get; set; }
        public KhachHangSearch KhachHangSearch { get; set; }
    }
}
