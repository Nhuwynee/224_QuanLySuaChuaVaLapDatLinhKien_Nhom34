using System;
using System.Collections.Generic;

namespace QLSuaChuaVaLapDat.Models;

public partial class Quan
{
    public string IdQuan { get; set; } = null!;

    public string? IdThanhPho { get; set; }

    public string TenQuan { get; set; } = null!;

    public virtual ThanhPho? IdThanhPhoNavigation { get; set; }

    public virtual ICollection<Phuong> Phuongs { get; set; } = new List<Phuong>();

}
