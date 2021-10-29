using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SmartHouseWPF
{
    public partial class smart_houseContext : DbContext
    {
        public virtual DbSet<Action> Action { get; set; }
        public virtual DbSet<Device> Device { get; set; }
        public virtual DbSet<DeviceGroup> DeviceGroup { get; set; }
        public virtual DbSet<Room> Room { get; set; }
        public virtual DbSet<Statistic> Statistic { get; set; }
        public virtual DbSet<UserGroup> UserGroup { get; set; }
        public virtual DbSet<UserSystem> UserSystem { get; set; }
        //конструктор без параметров
        public smart_houseContext()
           : base()
        {
            Database.EnsureCreated();
        }
        //конструктор с параметром - строка подключения
        public smart_houseContext(DbContextOptions<smart_houseContext> options) 
            :base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
            optionsBuilder.UseNpgsql(@"Host=localhost;Port=5432;Database=smart_house;Username=postgres;Password=22");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Action>(entity =>
            {
                entity.ToTable("action");

                entity.HasIndex(e => e.DeviceId)
                    .HasName("fki_fkey_act_id_dev");

                entity.HasIndex(e => e.RoomId)
                    .HasName("fki_fkey_act_id_room");

                entity.HasIndex(e => e.UserSystemId)
                    .HasName("fki_fkey _act_id_user");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DateTime)
                    .HasColumnName("date_time")
                    .HasColumnType("timestamptz");

                entity.Property(e => e.DelFlag)
                    .HasColumnName("del_flag")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

                entity.Property(e => e.RoomId).HasColumnName("room_id");

                entity.Property(e => e.UserSystemId).HasColumnName("user_system_id");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.Action)
                    .HasForeignKey(d => d.DeviceId)
                    .HasConstraintName("fkey_act_id_dev");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.Action)
                    .HasForeignKey(d => d.RoomId)
                    .HasConstraintName("fkey_act_id_room");

                entity.HasOne(d => d.UserSystem)
                    .WithMany(p => p.Action)
                    .HasForeignKey(d => d.UserSystemId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fkey _act_id_user");
            });

            modelBuilder.Entity<Device>(entity =>
            {
                entity.ToTable("device");

                entity.HasIndex(e => e.DeviceGroupId)
                    .HasName("fki_fkey_dev_dev_group");

                entity.HasIndex(e => e.Name)
                    .HasName("uni_dev_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DelFlag)
                    .HasColumnName("del_flag")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.DeviceGroupId).HasColumnName("device_group_id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar")
                    .HasMaxLength(64);

                entity.HasOne(d => d.DeviceGroup)
                    .WithMany(p => p.Device)
                    .HasForeignKey(d => d.DeviceGroupId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fkey_dev_dev_group");
            });

            modelBuilder.Entity<DeviceGroup>(entity =>
            {
                entity.ToTable("device_group");

                entity.HasIndex(e => e.Name)
                    .HasName("uni_dev_group_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DelFlag)
                    .HasColumnName("del_flag")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar")
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("room");

                entity.HasIndex(e => e.Name)
                    .HasName("uni_room_name")
                    .IsUnique();

                entity.HasIndex(e => e.SerialNumber)
                    .HasName("uni_room_serial_number")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DelFlag)
                    .HasColumnName("del_flag")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar")
                    .HasMaxLength(64);

                entity.Property(e => e.SerialNumber).HasColumnName("serial_number");
            });

            modelBuilder.Entity<Statistic>(entity =>
            {
                entity.ToTable("statistic");

                entity.HasIndex(e => e.DeviceId)
                    .HasName("fki_fkey_stat_id_dev");

                entity.HasIndex(e => e.RoomId)
                    .HasName("fki_fkey_stat_id_room");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('recorder_id_seq'::regclass)");

                entity.Property(e => e.DateTime)
                    .HasColumnName("date_time")
                    .HasColumnType("timestamptz");

                entity.Property(e => e.DelFlag)
                    .HasColumnName("del_flag")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

                entity.Property(e => e.Emergency)
                    .HasColumnName("emergency")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.RoomId).HasColumnName("room_id");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.Statistic)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fkey_stat_id_dev");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.Statistic)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fkey_stat_id_room");
            });

            modelBuilder.Entity<UserGroup>(entity =>
            {
                entity.ToTable("user_group");

                entity.HasIndex(e => e.Name)
                    .HasName("uni_user_group_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DelFalg)
                    .HasColumnName("del_falg")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar")
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<UserSystem>(entity =>
            {
                entity.ToTable("user_system");

                entity.HasIndex(e => e.FullName)
                    .HasName("uni_user_system_fn")
                    .IsUnique();

                entity.HasIndex(e => e.UserGroupId)
                    .HasName("fki_fkey_id_id_user_group");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('user_id_seq'::regclass)");

                entity.Property(e => e.DelFlag)
                    .HasColumnName("del_flag")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasColumnName("full_name")
                    .HasColumnType("varchar")
                    .HasMaxLength(255);

                entity.Property(e => e.Pswd)
                    .IsRequired()
                    .HasColumnName("pswd")
                    .HasColumnType("varchar")
                    .HasMaxLength(255);

                entity.Property(e => e.UserGroupId).HasColumnName("user_group_id");

                entity.HasOne(d => d.UserGroup)
                    .WithMany(p => p.UserSystem)
                    .HasForeignKey(d => d.UserGroupId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fkey_user_id_user_group");
            });

            modelBuilder.HasSequence("recorder_id_seq");

            modelBuilder.HasSequence("user_id_seq")
                .HasMin(1)
                .HasMax(2147483647);
        }
    }
}