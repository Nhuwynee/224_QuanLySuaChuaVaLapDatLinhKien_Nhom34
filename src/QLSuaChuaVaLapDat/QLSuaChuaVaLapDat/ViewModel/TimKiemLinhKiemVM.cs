
using QLSuaChuaVaLapDat.Models;
using QLSuaChuaVaLapDat.Models.TimKiem;

namespace QLSuaChuaVaLapDat.ViewModel
{
    public class TimKiemLinhKiemVM
    {
        public List<LinhKien> LinhKiens { get; set; }
        public List<NhaSanXuat> NhaSanXuats { get; set; }

        public Paging Paging { get; set; }
        public List<LoaiLinhKien> LoaiLinhKiens{ get; set; }
        public LinhKienSearch linhKienSearch { get; set; }
    }
}
