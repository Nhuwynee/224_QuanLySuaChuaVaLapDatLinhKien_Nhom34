namespace QLSuaChuaVaLapDat.Models.viewmodel
{
    public class User
{
    public string IdUser { get; set; }
    public string TenUser { get; set; }
    public string MatKhau { get; set; }
    public string HoVaTen { get; set; }
    public string IdRole { get; set; }
    public Role Role { get; set; }
    // ... các trường khác
}

public class Role
{
    public string IdRole { get; set; }
    public string TenRole { get; set; }
    public ICollection<User> Users { get; set; }
}

}
