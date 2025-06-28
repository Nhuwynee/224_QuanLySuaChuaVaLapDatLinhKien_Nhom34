using System;
using System.Collections.Generic;

namespace QLSuaChuaVaLapDat.Models;

public partial class Role
{
    public string IdRole { get; set; } = null!;

    public string TenRole { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();

}
