using QLSuaChuaVaLapDat.Models;
using QLSuaChuaVaLapDat.Models.TimKiem;

namespace QLSuaChuaVaLapDat.ViewModel
{
    public class BaoHanhSearchVM
    {
        public List<ChiTietDonHangDTO> ChiTietDonHangs { get; set; }
        public List<LoaiLinhKien> linhKiens { get; set; }
        public List<NhaSanXuat> nhaSanXuats { get; set; }
        public Paging Paging { get; set; }
        public BaoHanhSearch baoHanhSearch { set; get; }


    }
}
