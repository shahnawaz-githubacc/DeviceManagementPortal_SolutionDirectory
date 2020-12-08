using DeviceManagementPortal.Models.DomainModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManagementPortal.Infrastructure.Contracts
{
    public interface IDeviceManagementDbContext
    {
        int SaveChanges();
    }
}
