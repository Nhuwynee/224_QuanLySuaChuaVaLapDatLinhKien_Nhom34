namespace QLSuaChuaVaLapDat.Models.viewmodel
{
    public class DonDichVuPagedViewModel
    {
        public List<DonDichVuViewModel> DonDichVus { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
