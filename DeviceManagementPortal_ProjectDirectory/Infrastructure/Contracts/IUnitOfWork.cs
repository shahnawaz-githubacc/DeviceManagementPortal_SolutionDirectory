using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManagementPortal.Infrastructure.Contracts
{
    public interface IUnitOfWork
    {
        int Commit();

        IDeviceManagementDbContext GetContext();
    }
}
