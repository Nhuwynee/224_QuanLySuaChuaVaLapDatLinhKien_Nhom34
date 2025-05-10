using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QLSuaChuaVaLapDat.Models;

public partial class LoaiLoi
{
    [Key]
    public string IdLoi { get; set; } = null!;

    public string? MoTaLoi { get; set; }

}
