using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using DeviceManagementPortal.Models.DTOs;

namespace DeviceManagementPortal.Models.DomainModels
{
    public class Backend
    {
        [Key]
        public int BackendID { get; set; }

        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Address { get; set; }

        public ICollection<DeviceBackend> DeviceBackends { get; set; }

        public static Backend Map(BackendDTO backendDTO)
        {
            return new Backend
            {
                Name = backendDTO.Name,
            };
        }

        public static Backend MapApi(BackendApiDTO backendApiDTO)
        {
            return new Backend
            {
                Name = backendApiDTO.Name,
                Address = backendApiDTO.Address
            };
        }
    }
}
