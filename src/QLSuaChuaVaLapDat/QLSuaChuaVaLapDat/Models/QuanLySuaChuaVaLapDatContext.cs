﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QLSuaChuaVaLapDat.Models;

public class QuanLySuaChuaVaLapDatContext : DbContext
{
    public QuanLySuaChuaVaLapDatContext(DbContextOptions<QuanLySuaChuaVaLapDatContext> options)
        : base(options)
    {
    }

    public DbSet<User> User { get; set; }

    public DbSet<DonDichVu> DonDichVu { get; set; }

    public DbSet<DonGia> DonGia { get; set; }

    public DbSet<LoaiLoi> LoaiLoi { get; set; }

}