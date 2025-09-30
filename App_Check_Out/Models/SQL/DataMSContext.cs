using APP_CHECKOUT.Models.Orders;
using Entities.Models;
using HuloToys_Service.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace APP_CHECKOUT.Models.SQL
{
    public partial class DataMSContext : DbContext
    {
        public DataMSContext()
        {
        }

        public DataMSContext(DbContextOptions<DataMSContext> options)
            : base(options)
        {
        }


        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }

        public virtual DbSet<Voucher> Vouchers { get; set; }
        public virtual DbSet<OrderMerge> OrderMerges { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.Note).HasComment("Chính là label so với wiframe");
                entity.Property(e => e.OrderNo)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.UpdateLast).HasColumnType("datetime");
                entity.Property(e => e.UtmMedium).HasMaxLength(250);
                entity.Property(e => e.UserGroupIds).HasMaxLength(250);
                entity.Property(e => e.UtmSource).HasMaxLength(50);
            });
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetail");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.ProductCode)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.ProductLink)
                    .HasMaxLength(1000)
                    .IsUnicode(false);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });
            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.ToTable("Voucher");

                entity.Property(e => e.CampaignId).HasColumnName("campaign_id");
                entity.Property(e => e.Cdate)
                    .HasColumnType("datetime")
                    .HasColumnName("cdate");
                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("code");
                entity.Property(e => e.Description)
                    .HasMaxLength(4000)
                    .HasColumnName("description");
                entity.Property(e => e.EDate)
                    .HasColumnType("datetime")
                    .HasColumnName("eDate");
                entity.Property(e => e.GroupUserPriority)
                    .HasMaxLength(4000)
                    .IsUnicode(false)
                    .HasComment("Trường này để lưu nhóm những user được áp dụng trên voucher này")
                    .HasColumnName("group_user_priority");
                entity.Property(e => e.IsLimitVoucher).HasColumnName("is_limit_voucher");
                entity.Property(e => e.IsMaxPriceProduct).HasColumnName("is_max_price_product");
                entity.Property(e => e.IsPublic)
                    .HasComment("Nêu set true thì hiểu voucher này được public cho các user thanh toán đơn hàng")
                    .HasColumnName("is_public");
                entity.Property(e => e.LimitTotalDiscount).HasColumnName("limit_total_discount");
                entity.Property(e => e.LimitUse).HasColumnName("limitUse");
                entity.Property(e => e.MinTotalAmount).HasColumnName("min_total_amount");
                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .HasColumnName("name");
                entity.Property(e => e.PriceSales)
                    .HasColumnType("money")
                    .HasColumnName("price_sales");
                entity.Property(e => e.RuleType)
                    .HasComment("Trường này dùng để phân biệt voucher triển khai này chạy theo rule nào. Ví dụ: rule giảm giá với 1 số tiền vnđ trên toàn bộ đơn hàng. Giảm giá 20% phí first pound đầu tiên của nhãn hàng amazon. 1: triển khai rule giảm giá cho toàn bộ đơn hàng. 2 là rule áp dụng cho 20% phí first pound đầu tiên.")
                    .HasColumnName("rule_type");
                entity.Property(e => e.StoreApply)
                    .HasMaxLength(4000)
                    .IsUnicode(false)
                    .HasColumnName("store_apply");
                entity.Property(e => e.Udate)
                    .HasColumnType("datetime")
                    .HasColumnName("udate");
                entity.Property(e => e.Unit)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("unit");
            });

            modelBuilder.Entity<OrderMerge>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("OrderMerge");

                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.Note).HasComment("Chính là label so với wiframe");
                entity.Property(e => e.OrderNo)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.Phone)
                    .HasMaxLength(50)
                    .IsFixedLength();
                entity.Property(e => e.ReceiverName).HasMaxLength(150);
                entity.Property(e => e.RefundDate).HasColumnType("datetime");
                entity.Property(e => e.RefundReason).HasMaxLength(500);
                entity.Property(e => e.UpdateLast).HasColumnType("datetime");
                entity.Property(e => e.UserGroupIds).HasMaxLength(250);
                entity.Property(e => e.UtmMedium).HasMaxLength(250);
                entity.Property(e => e.UtmSource).HasMaxLength(50);
                entity.Property(e => e.VoucherId).HasMaxLength(50);
            });
            OnModelCreatingPartial(modelBuilder);


        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}
