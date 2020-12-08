using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using DeviceManagementPortal.Models.DTOs;
using DeviceManagementPortal.Infrastructure.API.Validators;

namespace DeviceManagementPortal.Models.DomainModels
{
    public class Device
    {
        [Key]
        public int DeviceID { get; set; }

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

        [Required]
        public DateTime CreatedDateTime { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        public ICollection<DeviceBackend> DeviceBackends { get; set; }

        public static Device Map(DeviceAddDTO deviceDTO, string userName)
        {
            return new Device
            {
                IMEI = deviceDTO.IMEI,
                Model = deviceDTO.Model,
                SimCardNumber = deviceDTO.SimCardNumber,
                Enabled = deviceDTO.Enabled,
                DeviceBackends = new List<DeviceBackend>(),

                CreatedBy = userName,
                CreatedDateTime = DateTime.Now
            };
        }
    }
}
