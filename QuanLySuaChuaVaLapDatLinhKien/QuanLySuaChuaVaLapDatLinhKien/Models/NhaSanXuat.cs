using System;
using System.Collections.Generic;

namespace QuanLySuaChuaVaLapDatLinhKien.Models;

public partial class NhaSanXuat
{
    public string IdNsx { get; set; } = null!;

    public string TenNsx { get; set; } = null!;

    public virtual ICollection<LinhKien> LinhKiens { get; set; } = new List<LinhKien>();
}
