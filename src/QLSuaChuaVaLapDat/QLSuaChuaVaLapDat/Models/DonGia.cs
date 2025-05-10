using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QLSuaChuaVaLapDat.Models;

public partial class DonGia
{
    [Key]
    public string IdDonGia { get; set; } = null!;

    public string? IdLoi { get; set; }

    public decimal Gia { get; set; }

    public DateOnly? NgayCapNhat { get; set; }

    public virtual LoaiLoi? IdLoiNavigation { get; set; }
}