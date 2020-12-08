using DeviceManagementPortal.Infrastructure.Contracts;
using DeviceManagementPortal.Infrastructure.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManagementPortal.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDeviceManagementDbContext DeviceManagementDbContext = null;
        public UnitOfWork(IDeviceManagementDbContext deviceManagementDbContext)
        {
            DeviceManagementDbContext = deviceManagementDbContext;
        }

        int IUnitOfWork.Commit()
        {
            return DeviceManagementDbContext.SaveChanges();
        }

        IDeviceManagementDbContext IUnitOfWork.GetContext() { return DeviceManagementDbContext; }
    }
}
