using QLSuaChuaVaLapDat.Models;
using QLSuaChuaVaLapDat.Models.TimKiem;

namespace QLSuaChuaVaLapDat.ViewModel
{
    public class TimKiemDichVuVM
    {
        public List<DonDichVu> DonDichVu { get; set; }
        public Paging Paging { get; set; }
        public List<ThietBi> loaiTB { set; get; }
        public DonDichVuSearch donDichVuSearch { set; get; }

    }
}
