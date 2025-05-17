using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QLSuaChuaVaLapDat.Models
{
    [Table("DonGia")]
    public class DonGia
    {
        [Key]
        [StringLength(7)]
        public string idDonGia { get; set; }

        [StringLength(7)]
        [ForeignKey(nameof(LoaiLoi))]
        public string idLoi { get; set; }
        public LoaiLoi LoaiLoi { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal gia { get; set; }

        public DateTime? ngayCapNhat { get; set; }
    }

}
