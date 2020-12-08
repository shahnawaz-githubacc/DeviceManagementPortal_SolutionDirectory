using DeviceManagementPortal.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManagementPortal.Models.DTOs
{
    public class DevicesWithPaginationInfo
    {
        public IEnumerable<Device> Devices { get; set; }
        public PaginationInfo PaginationInfo { get; set; }
    }
}
