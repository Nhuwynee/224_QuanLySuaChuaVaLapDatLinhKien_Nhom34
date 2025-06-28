using System;
using System.Collections.Generic;

namespace QLSuaChuaVaLapDat.Models;

public partial class HinhAnh
{
    public string IdHinhAnh { get; set; } = null!;

    public string? IdCtdh { get; set; }

    public string Anh { get; set; } = null!;

    public string LoaiHinhAnh { get; set; } = null!;

    public virtual ChiTietDonDichVu? IdCtdhNavigation { get; set; }

}

