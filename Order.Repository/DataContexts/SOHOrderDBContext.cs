using Microsoft.EntityFrameworkCore;
using Order.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository.DataContexts
{
    public class SOHOrderDBContext : DbContext
    {
        public SOHOrderDBContext(DbContextOptions<SOHOrderDBContext> options) : base(options) { }

        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<OrderItemDetail> OrderItemDetails { get; set; }
        public DbSet<LabelManagement> LabelManagements { get; set; }
        public DbSet<ManifestManagement> ManifestManagements { get; set; }
        public DbSet<PrescriptionDownloadRequest> PrescriptionDownloadRequests { get; set; }
        public DbSet<AppConfigurationSetting> AppConfigurationSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderDetail>()
               .HasMany(r => r.OrderItemDetails)
               .WithOne(s => s.OrderDetail)
               .HasForeignKey(f => f.orderid)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
