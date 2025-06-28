using System;
using System.Collections.Generic;

namespace QLSuaChuaVaLapDat.Models;

public partial class LoaiLinhKien
{
    public string IdLoaiLinhKien { get; set; } = null!;

    public string TenLoaiLinhKien { get; set; } = null!;

    public virtual ICollection<LinhKien> LinhKiens { get; set; } = new List<LinhKien>();

}

