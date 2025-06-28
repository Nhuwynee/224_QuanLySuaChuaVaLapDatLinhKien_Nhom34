using System;
using System.Collections.Generic;

namespace QLSuaChuaVaLapDat.Models;

public partial class AnhLinhKien
{
    public string IdAnh { get; set; } = null!;

    public string? IdLinhKien { get; set; }

    public string Anh { get; set; } = null!;

    public virtual LinhKien? IdLinhKienNavigation { get; set; }

}
