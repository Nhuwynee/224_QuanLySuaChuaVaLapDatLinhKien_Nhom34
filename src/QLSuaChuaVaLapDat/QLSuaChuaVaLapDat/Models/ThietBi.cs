using System;
using System.Collections.Generic;

namespace QLSuaChuaVaLapDat.Models;

public partial class ThietBi
{
    public string IdLoaiThietBi { get; set; } = null!;

    public string TenLoaiThietBi { get; set; } = null!;

    public virtual ICollection<DonDichVu> DonDichVus { get; set; } = new List<DonDichVu>();

}

