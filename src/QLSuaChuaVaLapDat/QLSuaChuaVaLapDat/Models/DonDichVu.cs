using System;
using System.Collections.Generic;

namespace QLSuaChuaVaLapDat.Models;

public partial class DonDichVu
{
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

    public virtual ICollection<ChiTietDonDichVu> ChiTietDonDichVus { get; set; } = new List<ChiTietDonDichVu>();

    public virtual ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();

    public virtual KhachVangLai? IdKhachVangLaiNavigation { get; set; }

    public virtual ThietBi IdLoaiThietBiNavigation { get; set; } = null!;

    public virtual User IdNhanVienKyThuatNavigation { get; set; } = null!;

    public virtual User? IdUserNavigation { get; set; }

    public virtual User IdUserTaoDonNavigation { get; set; } = null!;

}
