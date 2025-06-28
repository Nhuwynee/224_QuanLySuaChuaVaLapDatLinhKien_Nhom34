using System;
using System.Collections.Generic;

namespace QLSuaChuaVaLapDat.Models;

public partial class PhiLapDat
{
    public string IdPhiLapDat { get; set; } = null!;

    public string? IdLinhKien { get; set; }

    public decimal Phi { get; set; }

    public DateOnly? NgayApDung { get; set; }

    public virtual LinhKien? IdLinhKienNavigation { get; set; }

}
