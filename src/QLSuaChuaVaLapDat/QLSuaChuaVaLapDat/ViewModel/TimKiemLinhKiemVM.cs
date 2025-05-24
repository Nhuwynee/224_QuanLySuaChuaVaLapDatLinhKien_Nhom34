using QLSuaChuaVaLapDat.Models.Impl;
using QLSuaChuaVaLapDat.Models;

namespace QLSuaChuaVaLapDat.ViewModel
{
    public class TimKiemLinhKiemVM
    {
        public List<LinhKien> LinhKiens { get; set; }
        public List<NhaSanXuat> NhaSanXuats { get; set; }

        public List<LoaiLinhKien> LoaiLinhKiens{ get; set; }

    }
}
