using DeviceManagementPortal.Infrastructure.API.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManagementPortal.Models.DTOs
{
    public class DeviceAddDTO
    {
        [Required]
        [StringLength(20)]
        public string IMEI { get; set; }

        [Required]
        [StringLength(50)]
        public string Model { get; set; }

        [Required]
        [NumericMaxLengthValidation(20)]
        public long SimCardNumber { get; set; }

        public bool Enabled { get; set; }

        public List<BackendDTO> Backends { get; set; }
    }
}
