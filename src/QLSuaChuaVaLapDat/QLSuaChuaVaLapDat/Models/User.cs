using System;
using System.Collections.Generic;

namespace QLSuaChuaVaLapDat.Models;

public partial class User
{
    public string IdUser { get; set; } = null!;

    public string? IdRole { get; set; }

    public string? IdPhuong { get; set; }

    public string TenUser { get; set; } = null!;

    public string HoVaTen { get; set; } = null!;

    public string Sdt { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string? DiaChi { get; set; }

    //public DateOnly? NgaySinh { get; set; }

    public DateTime? NgaySinh { get; set; }


    public bool? TrangThai { get; set; }

    public string? Cccd { get; set; }

    public bool? GioiTinh { get; set; }

    public string? ChuyenMon { get; set; }

    public virtual ICollection<DonDichVu> DonDichVuIdNhanVienKyThuatNavigations { get; set; } = new List<DonDichVu>();

    public virtual ICollection<DonDichVu> DonDichVuIdUserNavigations { get; set; } = new List<DonDichVu>();

    public virtual ICollection<DonDichVu> DonDichVuIdUserTaoDonNavigations { get; set; } = new List<DonDichVu>();

    public virtual Phuong? IdPhuongNavigation { get; set; }

    public virtual Role? IdRoleNavigation { get; set; }

}

