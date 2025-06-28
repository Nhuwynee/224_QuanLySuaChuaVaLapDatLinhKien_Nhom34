using System;
using System.Collections.Generic;
using System.Globalization;

namespace QLSuaChuaVaLapDat.Models
{
	public class DoanhThuThangViewModel
	{
		public string ThangVaNam { get; set; } 
		public int Nam { get; set; }
		public int Thang { get; set; }
		public decimal TongDoanhThuTheoThang { get; set; }
		public double ChieuCaoCot { get; set; } 
	}

	public class ThongKeModel
    {
		public int TongDon { get; set; }
        public int TongDonCN { get; set; }
        public decimal TongDoanhTHu { get; set; }
        public int TongNhanVien { get; set; }
        public string ThangHT { get; set; }
		public decimal TongDoanhThuThangHT { get; set; }
        public DateTime ChartDateFocus { get; set; }
        public List<DonDichVu> DonDichVuGanNhat { get; set; }
        public string CurrentSearchString { get; set; }
        public int CurrentPage { get; set; } 
        public int TotalPages { get; set; } 

        public List<DoanhThuThangViewModel> DoanhThuThang { get; set; }

		public int KhachHang { get; set; }
		public int KhacVangLai { get; set; }
		public double PTKhachHang { get; set; }
		public double PTKhachVangLai { get; set; }

		public ThongKeModel()
		{
			DoanhThuThang = new List<DoanhThuThangViewModel>();
            ChartDateFocus = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); 
            DonDichVuGanNhat = new List<DonDichVu>();
			CurrentPage = 1;
        }
	}

	public class ChiTietThangViewModel
	{
		public string ThangVaNam { get; set; }
		public List<ThongTinKhachHangViewModel> ThongTinkh { get; set; }
		public ChiTietThangViewModel()
		{
			ThongTinkh = new List<ThongTinKhachHangViewModel>();
		}
	}

	public class ThongTinKhachHangViewModel
	{	
		public string Ten { get; set; }
		public decimal SoLuongTieu { get; set; }

	}



}