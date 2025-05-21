using System;
using System.Collections.Generic;
using System.Globalization;

namespace QLSuaChuaVaLapDat.Models
{
	public class DoanhThuThangViewModel
	{
		public string ThangVaNam { get; set; } // e.g., "Thg 5 2025"
		public int Nam { get; set; }
		public int Thang { get; set; }
		public decimal TongDoanhThuTheoThang { get; set; }
		public double ChieuCaoCot { get; set; } // For styling the bar height
	}

	public class ThongKeModel
    {
		// Existing Properties
		public int TongDon { get; set; }
        public decimal TongDoanhTHu { get; set; }
        public int TongNhanVien { get; set; }
        public string ThangHT { get; set; }
		public decimal TongDoanhThuThangHT { get; set; } // Revenue for a specific displayed month

		// For Revenue Chart
		public List<DoanhThuThangViewModel> DoanhThuThang { get; set; }

		// For Visitors Pie Chart
		public int KhachHang { get; set; }
		public int KhacVangLai { get; set; }
		public double PTKhachHang { get; set; }
		public double PTKhachVangLai { get; set; }

		public ThongKeModel()
		{
			DoanhThuThang = new List<DoanhThuThangViewModel>();
		}
	}

	// ViewModel for the drill-down customer details page
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
		// public string CustomerType { get; set; } // Optional: "Registered" or "Guest"
	}

}