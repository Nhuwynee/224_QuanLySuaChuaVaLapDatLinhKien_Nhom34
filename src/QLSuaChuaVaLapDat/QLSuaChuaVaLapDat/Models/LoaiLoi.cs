using System;
using System.Collections.Generic;

namespace QLSuaChuaVaLapDat.Models;

public partial class LoaiLoi
{
    public string IdLoi { get; set; } = null!;

    public string? MoTaLoi { get; set; }

    public virtual ICollection<ChiTietDonDichVu> ChiTietDonDichVus { get; set; } = new List<ChiTietDonDichVu>();

    public virtual ICollection<DonGia> DonGia { get; set; } = new List<DonGia>();

}

