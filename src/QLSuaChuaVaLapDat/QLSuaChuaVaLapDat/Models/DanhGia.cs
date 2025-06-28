using System;
using System.Collections.Generic;

namespace QLSuaChuaVaLapDat.Models;

public partial class DanhGia
{
    public string IdDanhGia { get; set; } = null!;

    public string? IdDonDichVu { get; set; }

    public int? DanhGiaNhanVien { get; set; }

    public int? DanhGiaDichVu { get; set; }

    public string? GopY { get; set; }

    public virtual DonDichVu? IdDonDichVuNavigation { get; set; }

}

