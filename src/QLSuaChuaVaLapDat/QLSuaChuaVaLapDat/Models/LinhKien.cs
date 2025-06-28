
﻿using System;
using System.Collections.Generic;
﻿using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace QLSuaChuaVaLapDat.Models;

public partial class LinhKien
{

    [Key]

    public string IdLinhKien { get; set; } = null!;

    public string? IdNsx { get; set; }

    public string? IdLoaiLinhKien { get; set; }


    [Required(ErrorMessage = "Tên linh kiện không được để trống")]
    public string TenLinhKien { get; set; } = null!;

    [Required(ErrorMessage = "Giá không được để trống")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
    public decimal Gia { get; set; }

    [Required(ErrorMessage = "Số lượng không được để trống")]
    [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]

    public int SoLuong { get; set; }

    public string? Anh { get; set; }


    [Required(ErrorMessage = "Thời gian bảo hành không được để trống")]
    [Range(0, 60, ErrorMessage = "Thời gian bảo hành phải từ 0 đến 60 tháng")]
    [Column(TypeName = "int")]
    public int ThoiGianBaoHanh { get; set; }

    [Required(ErrorMessage = "Điều kiện bảo hành không được để trống")]
    public string DieuKienBaoHanh { get; set; } = null!;

    [NotMapped]
    public IFormFile? ImageFile { get; set; }

    public virtual ICollection<AnhLinhKien> AnhLinhKiens { get; set; } = new List<AnhLinhKien>();

    public virtual ICollection<ChiTietDonDichVu> ChiTietDonDichVus { get; set; } = new List<ChiTietDonDichVu>();

    public virtual LoaiLinhKien? IdLoaiLinhKienNavigation { get; set; }

    public virtual NhaSanXuat? IdNsxNavigation { get; set; }

    public virtual ICollection<PhiLapDat> PhiLapDats { get; set; } = new List<PhiLapDat>();
}
