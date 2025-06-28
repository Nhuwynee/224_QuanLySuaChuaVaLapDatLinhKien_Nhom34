using System;
using System.Collections.Generic;

namespace QLSuaChuaVaLapDat.Models;

public partial class KhachVangLai
{
    public string IdKhachVangLai { get; set; } = null!;

    public string HoVaTen { get; set; } = null!;

    public string Sdt { get; set; } = null!;

    public string? DiaChi { get; set; }

    public string? IdPhuong { get; set; }

    public virtual ICollection<DonDichVu> DonDichVus { get; set; } = new List<DonDichVu>();

    public virtual Phuong? IdPhuongNavigation { get; set; }

}
