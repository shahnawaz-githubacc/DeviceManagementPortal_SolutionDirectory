using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManagementPortal.Models.DomainModels
{
    public class DeviceBackend
    {
        [Key]
        public int DeviceBackendID { get; set; }

        public int DeviceID { get; set; }

        [ForeignKey("DeviceID")]
        public Device Device { get; set; }

        public int BackendID { get; set; }

        [ForeignKey("BackendID")]
        public Backend Backend { get; set; }

        //[Required]
        //public DateTime MappedDateTime { get; }
    }
}
