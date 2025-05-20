using QLSuaChuaVaLapDat.Models.Impl;
using System.Security.Claims;

namespace QLSuaChuaVaLapDat.Models
{
    public partial class KhachHang
    {
        public List<NguoiDung> Users { get; set; }
        public List<KhachVangLai> KhachVangLais { get; set; }
    }
}
