using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QLSuaChuaVaLapDat.Models;

public partial class DonDichVu
{
    [Key]
    public string IdDonDichVu { get; set; } = null!;

    public string? IdUser { get; set; }

    public string? IdKhachVangLai { get; set; }

    public string IdNhanVienKyThuat { get; set; } = null!;

    public string IdUserTaoDon { get; set; } = null!;

    public string IdLoaiThietBi { get; set; } = null!;

    public string? TenThietBi { get; set; }

    public string LoaiKhachHang { get; set; } = null!;

    public DateTime? NgayTaoDon { get; set; }

    public DateTime? NgayHoanThanh { get; set; }

    public decimal? TongTien { get; set; }

    public string HinhThucDichVu { get; set; } = null!;

    public string LoaiDonDichVu { get; set; } = null!;

    public string? PhuongThucThanhToan { get; set; }

    public string TrangThaiDon { get; set; } = null!;

    public DateTime? NgayChinhSua { get; set; }

}
