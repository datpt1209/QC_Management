using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QC_Management.Models;

public partial class QcManagmentContext : DbContext
{
    public QcManagmentContext()
    {
    }

    public QcManagmentContext(DbContextOptions<QcManagmentContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<ControlInfo> ControlInfos { get; set; }

    public virtual DbSet<ControlInfoDetail> ControlInfoDetails { get; set; }

    public virtual DbSet<ControlType> ControlTypes { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<DeviceTest> DeviceTests { get; set; }

    public virtual DbSet<LevelQc> LevelQcs { get; set; }

    public virtual DbSet<ReResult> ReResults { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<UnitTable> UnitTables { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
       

        var connectionString = AppConfig.BuildConnectionString();

        optionsBuilder.UseSqlServer(connectionString);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC073108088C");

            entity.ToTable("Category");
        });

        modelBuilder.Entity<ControlInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ControlI__3214EC073DDC4181");

            entity.ToTable("ControlInfo");

            entity.Property(e => e.ExpirationDate).HasColumnType("datetime");
            entity.Property(e => e.Lot)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("LOT");
            entity.Property(e => e.ProductionDate).HasColumnType("datetime");

            entity.HasOne(d => d.IdControlTypeNavigation).WithMany(p => p.ControlInfos)
                .HasForeignKey(d => d.IdControlType)
                .HasConstraintName("FK_ControlInfo_ControlType");
        });

        modelBuilder.Entity<ControlInfoDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ControlI__3214EC074F524D96");

            entity.ToTable("ControlInfoDetail");

            entity.Property(e => e.Lot)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("LOT");
            entity.Property(e => e.MeanApp).HasColumnName("MeanAPP");
            entity.Property(e => e.MeanNsx).HasColumnName("MeanNSX");
            entity.Property(e => e.SdApp).HasColumnName("SdAPP");
            entity.Property(e => e.SdNsx).HasColumnName("SdNSX");

            entity.HasOne(d => d.IdControlInfoNavigation).WithMany(p => p.ControlInfoDetails)
                .HasForeignKey(d => d.IdControlInfo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ControlInfoDetail_ControlInfo");

            entity.HasOne(d => d.IdDeviceNavigation).WithMany(p => p.ControlInfoDetails)
                .HasForeignKey(d => d.IdDevice)
                .HasConstraintName("FK_ControlInfoDetail_Device");

            entity.HasOne(d => d.IdLevelNavigation).WithMany(p => p.ControlInfoDetails)
                .HasForeignKey(d => d.IdLevel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ControlIn__IdLev__2E1BDC42");

            entity.HasOne(d => d.IdTestNavigation).WithMany(p => p.ControlInfoDetails)
                .HasForeignKey(d => d.IdTest)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ControlInfoDetail_Test");
        });

        modelBuilder.Entity<ControlType>(entity =>
        {
            entity.ToTable("ControlType");

            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.IdCategoryNavigation).WithMany(p => p.ControlTypes)
                .HasForeignKey(d => d.IdCategory)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ControlType_Category");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("Device");

            entity.Property(e => e.Name).HasMaxLength(250);

            entity.HasOne(d => d.IdCategoryNavigation).WithMany(p => p.Devices)
                .HasForeignKey(d => d.IdCategory)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Device_Category");
        });

        modelBuilder.Entity<DeviceTest>(entity =>
        {
            entity.ToTable("Device_Test");

            entity.HasOne(d => d.IdDeviceNavigation).WithMany(p => p.DeviceTests)
                .HasForeignKey(d => d.IdDevice)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Device_Test_Device");

            entity.HasOne(d => d.IdTestNavigation).WithMany(p => p.DeviceTests)
                .HasForeignKey(d => d.IdTest)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Device_Test_Test");
        });

        modelBuilder.Entity<LevelQc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LevelQC__3214EC077164C6BF");

            entity.ToTable("LevelQC");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ReResult>(entity =>
        {
            entity.ToTable("Re_Result");

            entity.Property(e => e.Date).HasColumnType("date");

            entity.HasOne(d => d.IdDeviceNavigation).WithMany(p => p.ReResults)
                .HasForeignKey(d => d.IdDevice)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Re_Result_Device");

            entity.HasOne(d => d.IdLevelNavigation).WithMany(p => p.ReResults)
                .HasForeignKey(d => d.IdLevel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Re_Result_LevelQC");

            entity.HasOne(d => d.IdTestNavigation).WithMany(p => p.ReResults)
                .HasForeignKey(d => d.IdTest)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Re_Result_Test");
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Result__3214EC076E2759B0");

            entity.ToTable("Result");

            entity.Property(e => e.DateRun).HasColumnType("date");
            entity.Property(e => e.IdUser).HasColumnName("idUser");
            entity.Property(e => e.IndexQc).HasColumnName("index_QC");
            entity.Property(e => e.IsOutRange).HasColumnName("isOutRange");
            entity.Property(e => e.Result1).HasColumnName("Result");

            entity.HasOne(d => d.IdControlDetailNavigation).WithMany(p => p.Results)
                .HasForeignKey(d => d.IdControlDetail)
                .HasConstraintName("FK_Result_ControlInfoDetail");

            entity.HasOne(d => d.IdDeviceNavigation).WithMany(p => p.Results)
                .HasForeignKey(d => d.IdDevice)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Result_Device");

            entity.HasOne(d => d.IdLevelNavigation).WithMany(p => p.Results)
                .HasForeignKey(d => d.IdLevel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Result_LevelQC");

            entity.HasOne(d => d.IdTestNavigation).WithMany(p => p.Results)
                .HasForeignKey(d => d.IdTest)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Result_Test");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Results)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Result_User");
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Test__3214EC0705375ECF");

            entity.ToTable("Test");

            entity.HasOne(d => d.IdCategoryNavigation).WithMany(p => p.Tests)
                .HasForeignKey(d => d.IdCategory)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Test__IdCategory__267ABA7A");

            entity.HasOne(d => d.IdUnitTableNavigation).WithMany(p => p.Tests)
                .HasForeignKey(d => d.IdUnitTable)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Test_UnitTable");
        });

        modelBuilder.Entity<UnitTable>(entity =>
        {
            entity.ToTable("UnitTable");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisplayName).HasMaxLength(255);
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserName).HasMaxLength(255);

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.Role)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_UserRole");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRole");

            entity.Property(e => e.DisplayName).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
