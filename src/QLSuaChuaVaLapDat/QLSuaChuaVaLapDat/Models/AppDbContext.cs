using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QLSuaChuaVaLapDat.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AnhLinhKien> AnhLinhKiens { get; set; }

    public virtual DbSet<ChiTietDonDichVu> ChiTietDonDichVus { get; set; }

    public virtual DbSet<DanhGium> DanhGia { get; set; }

    public virtual DbSet<DonDichVu> DonDichVus { get; set; }

    public virtual DbSet<DonGium> DonGia { get; set; }

    public virtual DbSet<HinhAnh> HinhAnhs { get; set; }

    public virtual DbSet<KhachVangLai> KhachVangLais { get; set; }

    public virtual DbSet<LinhKien> LinhKiens { get; set; }

    public virtual DbSet<LoaiLinhKien> LoaiLinhKiens { get; set; }

    public virtual DbSet<LoaiLoi> LoaiLois { get; set; }

    public virtual DbSet<NhaSanXuat> NhaSanXuats { get; set; }

    public virtual DbSet<PhiLapDat> PhiLapDats { get; set; }

    public virtual DbSet<Phuong> Phuongs { get; set; }

    public virtual DbSet<Quan> Quans { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<ThanhPho> ThanhPhos { get; set; }

    public virtual DbSet<ThietBi> ThietBis { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-1FO0E7NF\\YNHUW;Database=QuanLySuaChuaVaLapDat;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnhLinhKien>(entity =>
        {
            entity.HasKey(e => e.IdAnh).HasName("PK__AnhLinhK__3E0F127B328D3198");

            entity.ToTable("AnhLinhKien");

            entity.HasIndex(e => e.Anh, "UQ__AnhLinhK__DE57AB49F3B76FE5").IsUnique();

            entity.Property(e => e.IdAnh)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idAnh");
            entity.Property(e => e.Anh)
                .HasMaxLength(200)
                .HasColumnName("anh");
            entity.Property(e => e.IdLinhKien)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idLinhKien");

            entity.HasOne(d => d.IdLinhKienNavigation).WithMany(p => p.AnhLinhKiens)
                .HasForeignKey(d => d.IdLinhKien)
                .HasConstraintName("FK__AnhLinhKi__idLin__60A75C0F");
        });

        modelBuilder.Entity<ChiTietDonDichVu>(entity =>
        {
            entity.HasKey(e => e.IdCtdh).HasName("PK__ChiTietD__06F5C5061DB5547B");

            entity.ToTable("ChiTietDonDichVu");

            entity.Property(e => e.IdCtdh)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idCTDH");
            entity.Property(e => e.HanBaoHanh).HasColumnName("hanBaoHanh");
            entity.Property(e => e.IdDonDichVu)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idDonDichVu");
            entity.Property(e => e.IdLinhKien)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idLinhKien");
            entity.Property(e => e.IdLoi)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idLoi");
            entity.Property(e => e.LoaiDichVu)
                .HasMaxLength(100)
                .HasColumnName("loaiDichVu");
            entity.Property(e => e.MoTa)
                .HasMaxLength(500)
                .HasColumnName("moTa");
            entity.Property(e => e.NgayKetThucBh).HasColumnName("ngayKetThucBH");
            entity.Property(e => e.SoLuong).HasColumnName("soLuong");
            entity.Property(e => e.ThoiGianThemLinhKien)
                .HasColumnType("datetime")
                .HasColumnName("thoiGianThemLinhKien");

            entity.HasOne(d => d.IdDonDichVuNavigation).WithMany(p => p.ChiTietDonDichVus)
                .HasForeignKey(d => d.IdDonDichVu)
                .HasConstraintName("FK__ChiTietDo__idDon__75A278F5");

            entity.HasOne(d => d.IdLinhKienNavigation).WithMany(p => p.ChiTietDonDichVus)
                .HasForeignKey(d => d.IdLinhKien)
                .HasConstraintName("FK__ChiTietDo__idLin__76969D2E");

            entity.HasOne(d => d.IdLoiNavigation).WithMany(p => p.ChiTietDonDichVus)
                .HasForeignKey(d => d.IdLoi)
                .HasConstraintName("FK__ChiTietDo__idLoi__778AC167");
        });

        modelBuilder.Entity<DanhGium>(entity =>
        {
            entity.HasKey(e => e.IdDanhGia).HasName("PK__DanhGia__EC1F248E74A83545");

            entity.Property(e => e.IdDanhGia)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idDanhGia");
            entity.Property(e => e.DanhGiaDichVu).HasColumnName("danhGiaDichVu");
            entity.Property(e => e.DanhGiaNhanVien).HasColumnName("danhGiaNhanVien");
            entity.Property(e => e.GopY)
                .HasColumnType("ntext")
                .HasColumnName("gopY");
            entity.Property(e => e.IdDonDichVu)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idDonDichVu");

            entity.HasOne(d => d.IdDonDichVuNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.IdDonDichVu)
                .HasConstraintName("FK__DanhGia__idDonDi__01142BA1");
        });

        modelBuilder.Entity<DonDichVu>(entity =>
        {
            entity.HasKey(e => e.IdDonDichVu).HasName("PK__DonDichV__88991731F71DB170");

            entity.ToTable("DonDichVu", tb =>
                {
                    tb.HasTrigger("tg_UpdateNgayHoanThanh");
                    tb.HasTrigger("trg_UpdateNgayChinhSua");
                });

            entity.Property(e => e.IdDonDichVu)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idDonDichVu");
            entity.Property(e => e.HinhThucDichVu)
                .HasMaxLength(100)
                .HasColumnName("hinhThucDichVu");
            entity.Property(e => e.IdKhachVangLai)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idKhachVangLai");
            entity.Property(e => e.IdLoaiThietBi)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idLoaiThietBi");
            entity.Property(e => e.IdNhanVienKyThuat)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idNhanVienKyThuat");
            entity.Property(e => e.IdUser)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idUser");
            entity.Property(e => e.IdUserTaoDon)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idUserTaoDon");
            entity.Property(e => e.LoaiDonDichVu)
                .HasMaxLength(100)
                .HasColumnName("loaiDonDichVu");
            entity.Property(e => e.LoaiKhachHang)
                .HasMaxLength(50)
                .HasColumnName("loaiKhachHang");
            entity.Property(e => e.NgayChinhSua)
                .HasColumnType("datetime")
                .HasColumnName("ngayChinhSua");
            entity.Property(e => e.NgayHoanThanh)
                .HasColumnType("datetime")
                .HasColumnName("ngayHoanThanh");
            entity.Property(e => e.NgayTaoDon)
                .HasColumnType("datetime")
                .HasColumnName("ngayTaoDon");
            entity.Property(e => e.PhuongThucThanhToan)
                .HasMaxLength(100)
                .HasColumnName("phuongThucThanhToan");
            entity.Property(e => e.TenThietBi)
                .HasMaxLength(150)
                .HasColumnName("tenThietBi");
            entity.Property(e => e.TongTien)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("tongTien");
            entity.Property(e => e.TrangThaiDon)
                .HasMaxLength(150)
                .HasColumnName("trangThaiDon");

            entity.HasOne(d => d.IdKhachVangLaiNavigation).WithMany(p => p.DonDichVus)
                .HasForeignKey(d => d.IdKhachVangLai)
                .HasConstraintName("FK__DonDichVu__idKha__6D0D32F4");

            entity.HasOne(d => d.IdLoaiThietBiNavigation).WithMany(p => p.DonDichVus)
                .HasForeignKey(d => d.IdLoaiThietBi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonDichVu__idLoa__6FE99F9F");

            entity.HasOne(d => d.IdNhanVienKyThuatNavigation).WithMany(p => p.DonDichVuIdNhanVienKyThuatNavigations)
                .HasForeignKey(d => d.IdNhanVienKyThuat)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonDichVu__idNha__6E01572D");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.DonDichVuIdUserNavigations)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK__DonDichVu__idUse__6C190EBB");

            entity.HasOne(d => d.IdUserTaoDonNavigation).WithMany(p => p.DonDichVuIdUserTaoDonNavigations)
                .HasForeignKey(d => d.IdUserTaoDon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonDichVu__idUse__6EF57B66");
        });

        modelBuilder.Entity<DonGium>(entity =>
        {
            entity.HasKey(e => e.IdDonGia).HasName("PK__DonGia__3C885D6E88746D1A");

            entity.Property(e => e.IdDonGia)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idDonGia");
            entity.Property(e => e.Gia)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("gia");
            entity.Property(e => e.IdLoi)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idLoi");
            entity.Property(e => e.NgayCapNhat).HasColumnName("ngayCapNhat");

            entity.HasOne(d => d.IdLoiNavigation).WithMany(p => p.DonGia)
                .HasForeignKey(d => d.IdLoi)
                .HasConstraintName("FK__DonGia__idLoi__68487DD7");
        });

        modelBuilder.Entity<HinhAnh>(entity =>
        {
            entity.HasKey(e => e.IdHinhAnh).HasName("PK__HinhAnh__4187C930AEDFF2F2");

            entity.ToTable("HinhAnh");

            entity.HasIndex(e => e.Anh, "UQ__HinhAnh__DE57AB49C0CE7468").IsUnique();

            entity.Property(e => e.IdHinhAnh)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idHinhAnh");
            entity.Property(e => e.Anh)
                .HasMaxLength(100)
                .HasColumnName("anh");
            entity.Property(e => e.IdCtdh)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idCTDH");
            entity.Property(e => e.LoaiHinhAnh)
                .HasMaxLength(50)
                .HasColumnName("loaiHinhAnh");

            entity.HasOne(d => d.IdCtdhNavigation).WithMany(p => p.HinhAnhs)
                .HasForeignKey(d => d.IdCtdh)
                .HasConstraintName("FK__HinhAnh__idCTDH__7E37BEF6");
        });

        modelBuilder.Entity<KhachVangLai>(entity =>
        {
            entity.HasKey(e => e.IdKhachVangLai).HasName("PK__KhachVan__B09A268E9D78DCF3");

            entity.ToTable("KhachVangLai");

            entity.HasIndex(e => e.Sdt, "UQ__KhachVan__CA1930A547C690B4").IsUnique();

            entity.Property(e => e.IdKhachVangLai)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idKhachVangLai");
            entity.Property(e => e.DiaChi)
                .HasMaxLength(200)
                .HasColumnName("diaChi");
            entity.Property(e => e.HoVaTen)
                .HasMaxLength(100)
                .HasColumnName("hoVaTen");
            entity.Property(e => e.IdPhuong)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idPhuong");
            entity.Property(e => e.Sdt)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("SDT");

            entity.HasOne(d => d.IdPhuongNavigation).WithMany(p => p.KhachVangLais)
                .HasForeignKey(d => d.IdPhuong)
                .HasConstraintName("FK__KhachVang__idPhu__4E88ABD4");
        });

        modelBuilder.Entity<LinhKien>(entity =>
        {
            entity.HasKey(e => e.IdLinhKien).HasName("PK__LinhKien__9B25B3DA6136CE16");

            entity.ToTable("LinhKien");

            entity.Property(e => e.IdLinhKien)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idLinhKien");
            entity.Property(e => e.Anh)
                .HasMaxLength(100)
                .HasColumnName("anh");
            entity.Property(e => e.DieuKienBaoHanh)
                .HasMaxLength(500)
                .HasColumnName("dieuKienBaoHanh");
            entity.Property(e => e.Gia)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("gia");
            entity.Property(e => e.IdLoaiLinhKien)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idLoaiLinhKien");
            entity.Property(e => e.IdNsx)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idNSX");
            entity.Property(e => e.SoLuong)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("soLuong");
            entity.Property(e => e.TenLinhKien)
                .HasMaxLength(100)
                .HasColumnName("tenLinhKien");
            entity.Property(e => e.ThoiGianBaoHanh).HasColumnName("thoiGianBaoHanh");

            entity.HasOne(d => d.IdLoaiLinhKienNavigation).WithMany(p => p.LinhKiens)
                .HasForeignKey(d => d.IdLoaiLinhKien)
                .HasConstraintName("FK__LinhKien__idLoai__571DF1D5");

            entity.HasOne(d => d.IdNsxNavigation).WithMany(p => p.LinhKiens)
                .HasForeignKey(d => d.IdNsx)
                .HasConstraintName("FK__LinhKien__idNSX__5629CD9C");
        });

        modelBuilder.Entity<LoaiLinhKien>(entity =>
        {
            entity.HasKey(e => e.IdLoaiLinhKien).HasName("PK__LoaiLinh__1F012EFCC422F246");

            entity.ToTable("LoaiLinhKien");

            entity.HasIndex(e => e.TenLoaiLinhKien, "UQ__LoaiLinh__EFA4547AD6638A72").IsUnique();

            entity.Property(e => e.IdLoaiLinhKien)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idLoaiLinhKien");
            entity.Property(e => e.TenLoaiLinhKien)
                .HasMaxLength(100)
                .HasColumnName("tenLoaiLinhKien");
        });

        modelBuilder.Entity<LoaiLoi>(entity =>
        {
            entity.HasKey(e => e.IdLoi).HasName("PK__LoaiLoi__3C7153C4761A5DDF");

            entity.ToTable("LoaiLoi");

            entity.Property(e => e.IdLoi)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idLoi");
            entity.Property(e => e.MoTaLoi)
                .HasMaxLength(200)
                .HasColumnName("moTaLoi");
        });

        modelBuilder.Entity<NhaSanXuat>(entity =>
        {
            entity.HasKey(e => e.IdNsx).HasName("PK__NhaSanXu__3FFE7887CCA48FBE");

            entity.ToTable("NhaSanXuat");

            entity.Property(e => e.IdNsx)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idNSX");
            entity.Property(e => e.TenNsx)
                .HasMaxLength(100)
                .HasColumnName("tenNSX");
        });

        modelBuilder.Entity<PhiLapDat>(entity =>
        {
            entity.HasKey(e => e.IdPhiLapDat).HasName("PK__PhiLapDa__6318B79D0964F7E8");

            entity.ToTable("PhiLapDat");

            entity.Property(e => e.IdPhiLapDat)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idPhiLapDat");
            entity.Property(e => e.IdLinhKien)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idLinhKien");
            entity.Property(e => e.NgayApDung).HasColumnName("ngayApDung");
            entity.Property(e => e.Phi)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("phi");

            entity.HasOne(d => d.IdLinhKienNavigation).WithMany(p => p.PhiLapDats)
                .HasForeignKey(d => d.IdLinhKien)
                .HasConstraintName("FK__PhiLapDat__idLin__5BE2A6F2");
        });

        modelBuilder.Entity<Phuong>(entity =>
        {
            entity.HasKey(e => e.IdPhuong).HasName("PK__Phuong__3DC15CF9E22D38E5");

            entity.ToTable("Phuong");

            entity.HasIndex(e => e.TenPhuong, "UQ__Phuong__8415EF3344FC610D").IsUnique();

            entity.Property(e => e.IdPhuong)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idPhuong");
            entity.Property(e => e.IdQuan)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idQuan");
            entity.Property(e => e.IdThanhPho)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idThanhPho");
            entity.Property(e => e.TenPhuong)
                .HasMaxLength(150)
                .HasColumnName("tenPhuong");

            entity.HasOne(d => d.IdQuanNavigation).WithMany(p => p.Phuongs)
                .HasForeignKey(d => d.IdQuan)
                .HasConstraintName("FK__Phuong__idQuan__3F466844");

            entity.HasOne(d => d.IdThanhPhoNavigation).WithMany(p => p.Phuongs)
                .HasForeignKey(d => d.IdThanhPho)
                .HasConstraintName("FK__Phuong__idThanhP__403A8C7D");
        });

        modelBuilder.Entity<Quan>(entity =>
        {
            entity.HasKey(e => e.IdQuan).HasName("PK__Quan__D037A482D723F1A1");

            entity.ToTable("Quan");

            entity.HasIndex(e => e.TenQuan, "UQ__Quan__C83399641F0973FE").IsUnique();

            entity.Property(e => e.IdQuan)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idQuan");
            entity.Property(e => e.IdThanhPho)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idThanhPho");
            entity.Property(e => e.TenQuan)
                .HasMaxLength(150)
                .HasColumnName("tenQuan");

            entity.HasOne(d => d.IdThanhPhoNavigation).WithMany(p => p.Quans)
                .HasForeignKey(d => d.IdThanhPho)
                .HasConstraintName("FK__Quan__idThanhPho__3B75D760");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole).HasName("PK__Role__E5045C54ABB7C59F");

            entity.ToTable("Role");

            entity.HasIndex(e => e.TenRole, "UQ__Role__95448DDA1780E6C7").IsUnique();

            entity.Property(e => e.IdRole)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idRole");
            entity.Property(e => e.TenRole)
                .HasMaxLength(150)
                .HasColumnName("tenRole");
        });

        modelBuilder.Entity<ThanhPho>(entity =>
        {
            entity.HasKey(e => e.IdThanhPho).HasName("PK__ThanhPho__904D94D75506AE14");

            entity.ToTable("ThanhPho");

            entity.HasIndex(e => e.TenThanhPho, "UQ__ThanhPho__11DD970AE930AB54").IsUnique();

            entity.Property(e => e.IdThanhPho)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idThanhPho");
            entity.Property(e => e.TenThanhPho)
                .HasMaxLength(150)
                .HasColumnName("tenThanhPho");
        });

        modelBuilder.Entity<ThietBi>(entity =>
        {
            entity.HasKey(e => e.IdLoaiThietBi).HasName("PK__ThietBi__473BF42DC192A6EE");

            entity.ToTable("ThietBi");

            entity.HasIndex(e => e.TenLoaiThietBi, "UQ__ThietBi__24BB342BC966960C").IsUnique();

            entity.Property(e => e.IdLoaiThietBi)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idLoaiThietBi");
            entity.Property(e => e.TenLoaiThietBi)
                .HasMaxLength(200)
                .HasColumnName("tenLoaiThietBi");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PK__User__3717C9822D3DA983");

            entity.ToTable("User");

            entity.HasIndex(e => e.TenUser, "UQ__User__C60985FFCABAEA8E").IsUnique();

            entity.HasIndex(e => e.Sdt, "UQ__User__CA1930A50FB82090").IsUnique();

            entity.Property(e => e.IdUser)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idUser");
            entity.Property(e => e.Cccd)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CCCD");
            entity.Property(e => e.ChuyenMon)
                .HasMaxLength(100)
                .HasColumnName("chuyenMon");
            entity.Property(e => e.DiaChi)
                .HasMaxLength(200)
                .HasColumnName("diaChi");
            entity.Property(e => e.GioiTinh)
                .HasDefaultValue(true)
                .HasColumnName("gioiTinh");
            entity.Property(e => e.HoVaTen)
                .HasMaxLength(100)
                .HasColumnName("hoVaTen");
            entity.Property(e => e.IdPhuong)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idPhuong");
            entity.Property(e => e.IdRole)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("idRole");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("matKhau");
            entity.Property(e => e.NgaySinh).HasColumnName("ngaySinh");
            entity.Property(e => e.Sdt)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("SDT");
            entity.Property(e => e.TenUser)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tenUser");
            entity.Property(e => e.TrangThai)
                .HasDefaultValue(true)
                .HasColumnName("trangThai");

            entity.HasOne(d => d.IdPhuongNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdPhuong)
                .HasConstraintName("FK__User__idPhuong__48CFD27E");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdRole)
                .HasConstraintName("FK__User__idRole__47DBAE45");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
