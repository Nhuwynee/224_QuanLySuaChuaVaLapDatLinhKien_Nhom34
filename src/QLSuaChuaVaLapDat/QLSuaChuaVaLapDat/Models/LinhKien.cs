using System;
using System.Collections.Generic;

namespace QLSuaChuaVaLapDat.Models;

public partial class LinhKien
{
    public string IdLinhKien { get; set; } = null!;

    public string? IdNsx { get; set; }

    public string? IdLoaiLinhKien { get; set; }

    public string TenLinhKien { get; set; } = null!;

    public decimal Gia { get; set; }

    public int SoLuong { get; set; }

    public string? Anh { get; set; }

    public int ThoiGianBaoHanh { get; set; }

    public string DieuKienBaoHanh { get; set; } = null!;

    public virtual ICollection<AnhLinhKien> AnhLinhKiens { get; set; } = new List<AnhLinhKien>();

    public virtual ICollection<ChiTietDonDichVu> ChiTietDonDichVus { get; set; } = new List<ChiTietDonDichVu>();

    public virtual LoaiLinhKien? IdLoaiLinhKienNavigation { get; set; }

    public virtual NhaSanXuat? IdNsxNavigation { get; set; }

    public virtual ICollection<PhiLapDat> PhiLapDats { get; set; } = new List<PhiLapDat>();
}
