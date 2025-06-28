using System;
using System.Collections.Generic;

namespace QLSuaChuaVaLapDat.Models;

public partial class ChiTietDonDichVu
{
    public string IdCtdh { get; set; } = null!;

    public string? IdDonDichVu { get; set; }

    public string? IdLinhKien { get; set; }

    public string? IdLoi { get; set; }


    public string LoaiDichVu { get; set; }
    // = null! cho biết rằng trường này không được phép null

//    public string LoaiDichVu { get; set; } = null!;


    public string? MoTa { get; set; }

    public int SoLuong { get; set; }

    //public DateOnly? NgayKetThucBh { get; set; } = new DateOnly(2025, 12, 31);

    public DateOnly? NgayKetThucBh { get; set; }


    public DateTime? ThoiGianThemLinhKien { get; set; }

    public bool? HanBaoHanh { get; set; }

    public virtual ICollection<HinhAnh> HinhAnhs { get; set; } = new List<HinhAnh>();

    public virtual DonDichVu? IdDonDichVuNavigation { get; set; }

    public virtual LinhKien? IdLinhKienNavigation { get; set; }

    public virtual LoaiLoi? IdLoiNavigation { get; set; }

}
