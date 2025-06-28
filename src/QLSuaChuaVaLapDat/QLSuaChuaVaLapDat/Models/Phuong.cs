using System;
using System.Collections.Generic;

namespace QLSuaChuaVaLapDat.Models;

public partial class Phuong
{
    public string IdPhuong { get; set; } = null!;

    public string? IdQuan { get; set; }

    public string? IdThanhPho { get; set; }

    public string TenPhuong { get; set; } = null!;

    public virtual Quan? IdQuanNavigation { get; set; }

    public virtual ThanhPho? IdThanhPhoNavigation { get; set; }

    public virtual ICollection<KhachVangLai> KhachVangLais { get; set; } = new List<KhachVangLai>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();

}
