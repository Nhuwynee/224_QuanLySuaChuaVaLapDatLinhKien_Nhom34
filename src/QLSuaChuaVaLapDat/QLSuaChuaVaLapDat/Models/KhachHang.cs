using QLSuaChuaVaLapDat.Models.Impl;
using System.Security.Claims;

namespace QLSuaChuaVaLapDat.Models
{
    public partial class KhachHang
    {
        public KhachHang()
        {
            Users = new List<NguoiDung>();
            KhachVangLais = new List<KhachVangLaiImpl>();
        }
        public List<NguoiDung> Users { get; set; }
        public List<KhachVangLaiImpl> KhachVangLais { get; set; }
    }
}
