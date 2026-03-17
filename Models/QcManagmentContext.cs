using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using QC_Management.Models;

namespace QC_Management;

public partial class QcManagmentContext : DbContext
{
    public QcManagmentContext()
    {
    }

    public QcManagmentContext(DbContextOptions<QcManagmentContext> options)
        : base(options)
    {
    }


    public virtual DbSet<CalDetail> CalDetails { get; set; }

    public virtual DbSet<CalInfor> CalInfors { get; set; }

    public virtual DbSet<CalResult> CalResults { get; set; }

    public virtual DbSet<CalType> CalTypes { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<ControlInfo> ControlInfos { get; set; }

    public virtual DbSet<ControlInfoDetail> ControlInfoDetails { get; set; }

    public virtual DbSet<ControlType> ControlTypes { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<DeviceTest> DeviceTests { get; set; }

    public virtual DbSet<LevelQc> LevelQcs { get; set; }

    public virtual DbSet<ReResult> ReResults { get; set; }

    public virtual DbSet<ReCalResult> ReCalResults { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<UnitTable> UnitTables { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<InternalError> InternalErrors { get; set; }

    public virtual DbSet<CorrectiveAction> CorrectiveActions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = AppConfig.BuildConnectionString();

        optionsBuilder.UseSqlServer(connectionString);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {


    modelBuilder.Entity<CalDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CalDetai__3214EC072D3A7912");

            entity.ToTable("CalDetail");

            entity.HasOne(d => d.IdCalInforNavigation).WithMany(p => p.CalDetails)
                .HasForeignKey(d => d.IdCalInfor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CalDetail__IdCal__5D95E53A");

            entity.HasOne(d => d.IdDeviceNavigation).WithMany(p => p.CalDetails)
                .HasForeignKey(d => d.IdDevice)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CalDetail_Device");

            entity.HasOne(d => d.IdTestNavigation).WithMany(p => p.CalDetails)
                .HasForeignKey(d => d.IdTest)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CalDetail__IdTes__5CA1C101");

        });

        modelBuilder.Entity<CalInfor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CalInfor__3214EC07A9D3AA96");

            entity.ToTable("CalInfor");

            entity.Property(e => e.CalLot)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CalLOT");
            entity.Property(e => e.ExpirationDate).HasColumnType("date");

            entity.HasOne(d => d.IdCalTypeNavigation).WithMany(p => p.CalInfors)
                .HasForeignKey(d => d.IdCalType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CalInfor_CalType");
        });

        modelBuilder.Entity<InternalError>(entity =>
        {
            entity.ToTable("InternalErrors");
            entity.HasKey(e => e.Id).HasName("PK_InternalErrors");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime2");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Lot).HasMaxLength(200);

            // New mapping for Cause (root reason) on InternalErrors
            entity.Property(e => e.Cause)
                .HasMaxLength(1000);

            // NOTE: columns RangeMin, RangeMax, MeanApp, SdApp were removed from the InternalErrors model
            // and should be dropped from the database. Use the provided SQL migration script to drop them.
            // The canonical source for mean/sd and computed ranges is ControlInfoDetail (linked by ControlInfoDetailId).

            entity.HasOne(d => d.ErroneousResult)
                .WithMany()
                .HasForeignKey(d => d.ErroneousResultId)
                .HasConstraintName("FK_InternalErrors_Results_ErroneousResultId")
                .OnDelete(DeleteBehavior.Restrict);

            // use the navigation property explicitly
            entity.HasOne(d => d.Test)
                .WithMany()
                .HasForeignKey(d => d.TestId)
                .HasConstraintName("FK_InternalErrors_Test")
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Device)
                .WithMany()
                .HasForeignKey(d => d.DeviceId)
                .HasConstraintName("FK_InternalErrors_Device")
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.ControlInfoDetail)
                .WithMany()
                .HasForeignKey(d => d.ControlInfoDetailId)
                .HasConstraintName("FK_InternalErrors_ControlInfoDetail")
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CorrectiveAction>(entity =>
        {
            entity.ToTable("CorrectiveActions");
            entity.HasKey(e => e.Id).HasName("PK_CorrectiveActions");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime2");
            entity.Property(e => e.ActionOwner).HasMaxLength(200);
            entity.Property(e => e.Outcome).HasMaxLength(50);

            // Map PreventiveAction column
            entity.Property(e => e.PreventiveAction)
                .HasMaxLength(1000);

            // map to the existing collection navigation on InternalError to avoid EF creating a shadow FK
            entity.HasOne(d => d.InternalError)
                .WithMany(p => p.CorrectiveActions)
                .HasForeignKey(d => d.InternalErrorId)
                .HasConstraintName("FK_CorrectiveActions_InternalErrors_InternalErrorId")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.ResolvingResult)
                .WithMany()
                .HasForeignKey(d => d.ResolvingResultId)
                .HasConstraintName("FK_CorrectiveActions_Results_ResolvingResultId")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CalResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CalResul__3214EC0774A3814A");

            entity.ToTable("CalResult");

            entity.Property(e => e.DateRun).HasColumnType("date");

            entity.HasOne(d => d.IdCalDetailNavigation).WithMany(p => p.CalResults)
                .HasForeignKey(d => d.IdCalDetail)
                .HasConstraintName("FK__CalResult__IdCal__5E8A0973");

            entity.HasOne(d => d.IdDeviceNavigation).WithMany(p => p.CalResults)
                .HasForeignKey(d => d.IdDevice)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CalResult__Resul__4F47C5E3");

            entity.HasOne(d => d.IdTestNavigation).WithMany(p => p.CalResults)
                .HasForeignKey(d => d.IdTest)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CalResult__IdTes__503BEA1C");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.CalResults)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK_CalResult_User");
        });

        modelBuilder.Entity<CalType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CalType__3214EC0729EADF4B");

            entity.ToTable("CalType");

            entity.Property(e => e.CalTypeName).HasMaxLength(100);
        });


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
                .OnDelete(DeleteBehavior.ClientSetNull)
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

            // --- NEW mapping ---
            entity.Property(e => e.MeanSdUpdatedAt)
                .HasColumnName("MeanSdUpdatedAt")
                .HasColumnType("datetime");

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

            // Map new WestgardRulesJson column (stores JSON array of enabled rule keys)
            entity.Property(e => e.WestgardRulesJson)
                .HasColumnName("WestgardRulesJson")
                .HasColumnType("nvarchar(max)");
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

        modelBuilder.Entity<ReCalResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ReCalRes__3214EC074CF81388");

            entity.ToTable("ReCalResult");

            entity.Property(e => e.DateRun).HasColumnType("datetime");

            entity.HasOne(d => d.IdDeviceNavigation).WithMany(p => p.ReCalResults)
                .HasForeignKey(d => d.IdDevice)
                .HasConstraintName("FK__ReCalResu__Resul__56E8E7AB");

            entity.HasOne(d => d.IdTestNavigation).WithMany(p => p.ReCalResults)
                .HasForeignKey(d => d.IdTest)
                .HasConstraintName("FK__ReCalResu__IdTes__57DD0BE4");
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Result__3214EC076E2759B0");

            entity.ToTable("Result");

            entity.Property(e => e.DateRun).HasColumnType("datetime");
            entity.Property(e => e.IdUser).HasColumnName("idUser");
            entity.Property(e => e.IndexQc).HasColumnName("index_QC");
            entity.Property(e => e.IsOutRange).HasColumnName("isOutRange");
            entity.Property(e => e.Result1).HasColumnName("Result");

            // map IsCorrected to a bit column (nullable)
            entity.Property(e => e.IsCorrected).HasColumnName("IsCorrected").HasColumnType("bit");

            // --- NEW: persisted applied mean/sd/at columns ---
            entity.Property(e => e.AppliedMean)
                .HasColumnName("AppliedMean")
                .HasColumnType("float");
            entity.Property(e => e.AppliedSd)
                .HasColumnName("AppliedSd")
                .HasColumnType("float");
            entity.Property(e => e.AppliedAt)
                .HasColumnName("AppliedAt")
                .HasColumnType("datetime");

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
            entity.HasOne(d => d.TestTypeNavigation).WithMany(p => p.Tests)
               .HasForeignKey(d => d.TestType)
               .HasConstraintName("FK_Test_TestType");
        });

        modelBuilder.Entity<TestType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TestType__3214EC07701E803A");

            entity.ToTable("TestType");

            entity.Property(e => e.Name).HasMaxLength(50);
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
