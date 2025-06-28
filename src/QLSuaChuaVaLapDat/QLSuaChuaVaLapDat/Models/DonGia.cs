using System;
using System.Collections.Generic;

namespace QLSuaChuaVaLapDat.Models;

public partial class DonGia
{
    public string IdDonGia { get; set; } = null!;

    public string? IdLoi { get; set; }

    public decimal Gia { get; set; }

    public DateOnly? NgayCapNhat { get; set; }

    public virtual LoaiLoi? IdLoiNavigation { get; set; }

}

