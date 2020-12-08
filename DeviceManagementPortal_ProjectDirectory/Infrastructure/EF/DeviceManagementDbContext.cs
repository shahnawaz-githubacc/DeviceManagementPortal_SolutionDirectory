using DeviceManagementPortal.Infrastructure.Contracts;
using DeviceManagementPortal.Models.DomainModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManagementPortal.Infrastructure.EF
{
    public class DeviceManagementDbContext : DbContext, IDeviceManagementDbContext
    {
        public DeviceManagementDbContext(DbContextOptions<DeviceManagementDbContext> options)
            : base(options)
        {
        
        }

        public DbSet<Device> Devices { get; set; }
        public DbSet<Backend> Backends { get; set; }
        public DbSet<DeviceBackend> DeviceBackends { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>().HasIndex(index => index.IMEI).IsUnique();
        }
    }
}
