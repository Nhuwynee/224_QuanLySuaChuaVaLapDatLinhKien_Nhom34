using QLSuaChuaVaLapDat.Models;

namespace QLSuaChuaVaLapDat.ViewModel
{
    public class TìmKiemDichVuVM
    {
        public List<LinhKien> LinhKiens { get; set; }
        public List<NhaSanXuat> NhaSanXuats { get; set; }

        public List<LoaiLinhKien> LoaiLinhKiens { get; set; }
    }
}
