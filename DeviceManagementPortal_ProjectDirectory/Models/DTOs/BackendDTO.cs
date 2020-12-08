using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManagementPortal.Models.DTOs
{
    public class BackendDTO
    {
        public int BackendID { get; set; }

        [Required]
        [StringLength(20)]
        public string Name { get; set; }
    }
}
