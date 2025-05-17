using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QLSuaChuaVaLapDat.Models;
[Table("DanhGia")]
public class DanhGia
{
    [Key]
    [StringLength(7)]
    public string IdDanhGia { get; set; }

    [Required]
    [StringLength(7)]
    public string IdDonDichVu { get; set; }

    [Range(1, 5)]
    public int? DanhGiaNhanVien { get; set; }

    [Range(1, 5)]
    public int? DanhGiaDichVu { get; set; }

    public string GopY { get; set; }

    [ForeignKey("IdDonDichVu")]
    public virtual DonDichVu DonDichVu { get; set; }
}
