using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace QLSuaChuaVaLapDat.Models.viewmodel
{
    public class ServiceOrderFormViewModel
    {
        // Thông tin khách hàng
        public string IdKhachVangLai { get; set; }
        public string IdUser { get; set; }
        public string HoVaTen { get; set; }
        public string Sdt { get; set; }
        public string Email { get; set; }
        public string IdPhuong { get; set; }
        public string DuongSoNha { get; set; }
        
        // Thông tin thiết bị
        public string IdLoaiThietBi { get; set; }
        public string TenThietBi { get; set; }
        
        // Thông tin lỗi và linh kiện
        public List<ErrorDetailViewModel> ErrorDetails { get; set; }
        public List<string> SelectedPartIds { get; set; }
        public List<string> SelectedPartLoiIds { get; set; }

        // Thông tin dịch vụ
        public string LoaiDonDichVu { get; set; } // Sửa chữa hoặc lắp đặt
        public string LoaiDichVu { get; set; } // ở ctddv
        public string HinhThucDichVu { get; set; } // Tại nhà hoặc trực tiếp
        
        // Thông tin nhân viên
        public string IdNhanVienKyThuat { get; set; }
        
        // Thông tin ngày giờ
        public DateTime? NgayHoanThanh { get; set; }

        public DateOnly? NgayKetThucBaoHanh { get; set; } // Ngày kết thúc bảo hành

        // Thông tin thanh toán
        public decimal TongTien { get; set; }
        public decimal TienKhachHangUngTruoc { get; set; }
        
        // Thông tin đơn hàng
        public string IdDonDichVu { get; set; }
        public string TrangThaiDon { get; set; }

        public string MoTa { get; set; }
        
        // Thông tin ảnh
        public List<IFormFile> DeviceImages { get; set; }
        public List<IFormFile> WarrantyImages { get; set; }
    }

    public class ErrorDetailViewModel
    {
        public string IdLoi{ get; set; }
        public string MoTaLoi { get; set; }
        public string? IdLinhKien { get; set; }
        public int SoLuong { get; set; } = 1;
        public bool ConBaoHanh { get; set; }
        public DateOnly? NgayKetThucBaoHanh { get; set; }// Ngày kết thúc bảo hành
        public int ThoiGianBaoHanh { get; set; }
        public string GhiChu { get; set; }
        // Navigation properties to provide stronger typing
        public virtual LoaiLoi? Loi { get; set; }
        public virtual LinhKien? LinhKien { get; set; }
    }
}