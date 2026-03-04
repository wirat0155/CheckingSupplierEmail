using PurchasePortal.Models.DbModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchasePortal.Data
{
    public class ERPDbContext : DbContext
    {
        public ERPDbContext(DbContextOptions<ERPDbContext> options) : base(options)
        {

        }
        public DbSet<VEN> VEN { get; set; } // Vendor Supplier
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
