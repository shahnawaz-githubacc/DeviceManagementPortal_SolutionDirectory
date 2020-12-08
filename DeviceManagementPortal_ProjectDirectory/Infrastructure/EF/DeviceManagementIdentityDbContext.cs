using DeviceManagementPortal.Models.DomainModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManagementPortal.Infrastructure.EF
{
    public class DeviceManagementIdentityDbContext : IdentityDbContext<IdentityUser>
    {
        public DeviceManagementIdentityDbContext(DbContextOptions<DeviceManagementIdentityDbContext> options) : base(options) { }
    }
}
