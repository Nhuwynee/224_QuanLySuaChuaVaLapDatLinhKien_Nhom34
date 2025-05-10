using System;

namespace QLSuaChuaVaLapDat.Models
{
    public class ThongKeModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalStaff { get; set; }
        public string CurrentMonth { get; set; }
    }
}