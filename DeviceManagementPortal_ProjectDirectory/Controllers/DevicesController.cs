using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeviceManagementPortal.Infrastructure.API.Filters;
using DeviceManagementPortal.Infrastructure.Contracts;
using DeviceManagementPortal.Models;
using DeviceManagementPortal.Models.DomainModels;
using DeviceManagementPortal.Models.DTOs;
using DeviceManagementPortal.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DeviceManagementPortal.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        #region --- Global Variables ---

        private readonly IUnitOfWork UnitOfWork = null;
        private readonly IRepository<Device> Repository = null;
        private ILogger<DevicesController> Logger = null;
        private IConfiguration Configuration = null;

        #endregion --- Global Variables ---

        #region --- Constructor ---

        public DevicesController(IUnitOfWork unitOfWork, IRepository<Device> repository, ILogger<DevicesController> logger, IConfiguration configuration)
        {
            UnitOfWork = unitOfWork;
            Repository = repository;
            Logger = logger;
            Configuration = configuration;

            Logger.LogInformation("DeviceController : Parameterized Constructor Execution Finished.");
        }

        #endregion --- Constructor ---

        #region --- Actions ---

        [HttpGet("all", Name = "GetAllDevices")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAllDevices(int page = 1)
        {
            IActionResult actionResult;
            Logger.LogInformation("DeviceController : GetAllDevices : Execution Start.");
            Logger.LogDebug($"DeviceController : GetAllDevices : Parameter 'page', Value : {page}");
            try
            {
                int pageSize = Configuration.ReadAndParseIntConfigurationKeyValue("ApplicationLevelConfigurations:PageSize", 5);
                Logger.LogDebug($"DeviceController : GetAllDevices : PageSize : {pageSize}.");

                Logger.LogInformation("DeviceController : GetAllDevices : Before invoking Repository.GetAll().");
                IQueryable<Device> devices = Repository.GetAll();
                Logger.LogInformation("DeviceController : GetAllDevices : After invoking Repository.GetAll().");

                int totalItemCount = devices?.Count() ?? 0;
                Logger.LogDebug($"DeviceController : GetAllDevices : Total item count : {totalItemCount}");
                if (devices?.Count() > 0)
                {
                    DevicesWithPaginationInfo devicesWithPagination = new DevicesWithPaginationInfo
                    {
                        Devices = devices.Skip((page - 1) * pageSize).Take(pageSize).OrderBy(o => o.DeviceID).ToList(),
                        PaginationInfo = new PaginationInfo
                        {
                            CurrentPage = page,
                            PageSize = pageSize,
                            TotalPages = (int)(Math.Ceiling((decimal)totalItemCount / pageSize))
                        }
                    };

                    Logger.LogDebug($"DeviceController : GetAllDevices : Devices found in DB : {devices.Count()}");
                    actionResult = Ok(devicesWithPagination);
                }
                else
                {
                    Logger.LogInformation($"DeviceController : GetAllDevices : No devices found in DB.");
                    actionResult = Ok("No devices found in Database.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"DeviceController : GetAllDevices : Error occured. Error Message : {ex.Message}, Stack Trace : {ex.StackTrace}");
                throw;
            }
            finally
            {
                Logger.LogInformation("DeviceController : GetAllDevices : Execution Finished.");
            }
            return actionResult;
        }

        [HttpGet("get/{id:int}", Name = "GetDevice")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetDevice(int id)
        {
            IActionResult actionResult;
            Logger.LogInformation("DeviceController : GetDevice : Execution Start.");
            Logger.LogDebug($"DeviceController : GetDevice : Parameter 'id', Value : {id}");
            try
            {
                Logger.LogInformation("DeviceController : GetDevice : Before invoking Repository.GetAll().");
                Device device = Repository.GetAll()
                                    .Include(p => p.DeviceBackends)
                                        .ThenInclude(p => p.Backend)
                                            .FirstOrDefault(o => o.DeviceID == id);
                Logger.LogInformation("DeviceController : GetDevice : After invoking Repository.GetAll().");

                if (device != null)
                {
                    Logger.LogInformation("DeviceController : GetDevice : Item found in Database.");
                    if (device.DeviceBackends?.Count > 0)
                    {
                        Logger.LogDebug($"DeviceController : GetDevice : Link to Device Backend found, link count : {device.DeviceBackends.Count}");
                        foreach (var deviceBackend in device.DeviceBackends)
                        {
                            // Prevent circular reference.
                            deviceBackend.Device = null;
                            deviceBackend.Backend.DeviceBackends = null;
                        }
                    }

                    Logger.LogInformation($"DeviceController : GetDevice : Returning HTTP 200.");
                    actionResult = Ok(device);
                }
                else
                {
                    Logger.LogInformation("DeviceController : GetDevice : Item not found in Database.");
                    actionResult = NotFound($"Device with ID : {id} not found in Database.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"DeviceController : GetDevice : Error occured. Error Message : {ex.Message}, Stack Trace : {ex.StackTrace}");
                throw;
            }
            finally
            {
                Logger.LogInformation("DeviceController : GetDevice : Execution Finished.");
            }
            return actionResult;
        }

        [HttpGet("check/{imei}", Name = "CheckImei")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult CheckImei(string imei)
        {
            IActionResult actionResult;
            try
            {
                Logger.LogInformation("DeviceController : CheckImei : Execution Start.");
                Logger.LogDebug($"DeviceController : CheckImei : Parameter : {0}", imei);

                Logger.LogInformation("DeviceController : CheckImei : Before invoking Repository.GetAll() with Filtration.");
                Device devices = Repository.GetAll().FirstOrDefault(o => string.Equals(o.IMEI, imei));
                Logger.LogInformation("DeviceController : CheckImei : After invoking Repository.GetAll() with Filtration.");

                if (devices == null)
                {
                    Logger.LogInformation("DeviceController : CheckImei : No entry found, valid.");
                    actionResult = Ok("VALID");
                }
                else
                {
                    Logger.LogInformation("DeviceController : CheckImei : Entry found, invalid.");
                    actionResult = Ok("INVALID");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"DeviceController : CheckImei : Error occured. Error Message : {ex.Message}, Stack Trace : {ex.StackTrace}");
                throw;
            }
            finally
            {
                Logger.LogInformation("DeviceController : CheckImei : Execution Finished.");
            }
            return actionResult;
        }

        [HttpDelete("delete/{id:int}", Name = "DeleteDevice")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            IActionResult actionResult;
            Logger.LogInformation("DeviceController : DeleteDevice : Execution Start.");
            Logger.LogDebug($"DeviceController : DeleteDevice : Parameter 'id', Value : {id}.");
            try
            {
                Logger.LogInformation("DeviceController : DeleteDevice : Before invoking Repository.Get().");
                Device deviceToBeDeleted = await Repository.Get(id);
                Logger.LogInformation("DeviceController : DeleteDevice : After invoking Repository.Get().");

                if (deviceToBeDeleted != null)
                {
                    Logger.LogInformation("DeviceController : DeleteDevice : Before invoking Repository.Delete().");
                    Repository.Delete(deviceToBeDeleted);
                    Logger.LogInformation("DeviceController : DeleteDevice : After invoking Repository.Delete().");

                    Logger.LogInformation("DeviceController : DeleteDevice : Before invoking UnitOfWork.Commit().");
                    UnitOfWork.Commit();
                    Logger.LogInformation("DeviceController : DeleteDevice : After invoking UnitOfWork.Commit().");

                    actionResult = Ok($"Device with ID : {id} deleted from Database.");
                }
                else
                {
                    Logger.LogDebug($"DeviceController : DeleteDevice : Item with DeviceID : {id} not found.");
                    actionResult = NotFound("Device with Id : {id} not found.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"DeviceController : DeleteDevice : Error occured. Error Message : {ex.Message}, Stack Trace : {ex.StackTrace}");
                throw;
            }
            finally
            {
                Logger.LogInformation("DeviceController : DeleteDevice : Execution End.");
            }
            return actionResult;
        }

        [HttpPatch("updateStatus/{id:int}", Name = "UpdateDeviceStatus")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDeviceStatus(int id, [FromBody] JsonPatchDocument<Device> jsonPatchDocument)
        {
            IActionResult actionResult;
            try
            {
                Logger.LogInformation("DeviceController : UpdateDeviceStatus : Execution Start.");
                Logger.LogDebug($"DeviceController : UpdateDeviceStatus : Parameter : 'id', Value : {id}.");
                if (jsonPatchDocument?.Operations?.Count == 1)
                {
                    Logger.LogInformation("DeviceController : UpdateDeviceStatus : Update operation count = 0.");
                    Logger.LogInformation("DeviceController : UpdateDeviceStatus : Before invoking Repository.Get().");
                    Device deviceToBeUpdated = await Repository.Get(id);
                    Logger.LogInformation("DeviceController : UpdateDeviceStatus : Before invoking Repository.Get().");

                    if (deviceToBeUpdated != null)
                    {
                        Logger.LogInformation("DeviceController : UpdateDeviceStatus : Before applying patch.");
                        jsonPatchDocument.ApplyTo(deviceToBeUpdated);
                        Logger.LogInformation("DeviceController : UpdateDeviceStatus : After applying patch.");

                        if (ModelState.IsValid)
                        {
                            Logger.LogInformation("DeviceController : UpdateDeviceStatus : ModelState valid.");
                            Logger.LogInformation("DeviceController : UpdateDeviceStatus : Before committing to Database.");
                            UnitOfWork.Commit();
                            Logger.LogInformation("DeviceController : UpdateDeviceStatus : After committing to Database.");
                            actionResult = Ok($"Device with ID : {id} updated successfully.");
                        }
                        else
                        {
                            Logger.LogInformation("DeviceController : UpdateDeviceStatus : ModelState invalid.");
                            actionResult = new ObjectResult(ModelState);
                        }
                    }
                    else
                    {
                        Logger.LogInformation($"DeviceController : UpdateDeviceStatus : Device with ID : {id} not found.");
                        actionResult = NotFound($"Device with ID : {id} not found in Database.");
                    }
                }
                else
                {
                    Logger.LogInformation("DeviceController : UpdateDeviceStatus : Update operation count != 0. Bad Request.");
                    actionResult = BadRequest();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"DeviceController : UpdateDeviceStatus : Error occured. Error Message : {ex.Message}, Stack Trace : {ex.StackTrace}");
                throw;
            }
            finally
            {
                Logger.LogInformation("DeviceController : DeleteDevice : Execution End.");
            }
            return actionResult;
        }

        [HttpPost("add", Name = "AddDevice")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddDevice(DeviceAddDTO deviceAddDTO, [FromServices] IRepository<DeviceBackend> repository)
        {
            IActionResult actionResult;
            try
            {
                Logger.LogInformation("DeviceController : AddDevice : Execution Begin.");
                string userName = HttpContext.User.Identity.Name;
                Logger.LogDebug($"DeviceController : AddDevice : Identity User Name : {userName}");
                Device device = Device.Map(deviceAddDTO, userName);
                if (device != null)
                {
                    if (deviceAddDTO.Backends?.Count > 0)
                    {
                        Logger.LogInformation("DeviceController : AddDevice : Mapping with Backend exists.");
                        deviceAddDTO.Backends.ForEach(deviceBackendEntry =>
                        {
                            repository.Add(new DeviceBackend { BackendID = deviceBackendEntry.BackendID, Device = device });
                            Logger.LogInformation($"DeviceController : AddDevice : Mapping configured : Device (To be created) to Backend ID: {1}", deviceBackendEntry.BackendID);
                        });
                    }

                    Logger.LogInformation("DeviceController : AddDevice : Before invoking Repository.Add().");
                    Repository.Add(device);
                    Logger.LogInformation("DeviceController : AddDevice : Before invoking Repository.Add().");

                    Logger.LogInformation("DeviceController : AddDevice : Before committing to Database.");
                    UnitOfWork.Commit();
                    Logger.LogInformation("DeviceController : AddDevice : After committing to Database.");

                    if (device.DeviceID > 0)
                    {
                        Logger.LogDebug($"DeviceController : AddDevice : Newly created Device ID : {0}", device.DeviceID); 
                        if (device.DeviceBackends?.Count > 0)
                        {
                            Logger.LogDebug($"DeviceController : AddDevice : Link to Device Backend exists, link count : {device.DeviceBackends.Count}");
                            foreach (var deviceBackend in device.DeviceBackends)
                            {
                                // Prevent circular reference.
                                deviceBackend.Device = null;
                                if (deviceBackend.Backend != null)
                                    deviceBackend.Backend.DeviceBackends = null;
                            }
                        }

                        actionResult = Ok(device);
                        Logger.LogInformation("DeviceController : AddDevice : HTTP 200.");
                    }
                    else
                    {
                        actionResult = new ObjectResult("Error occured while saving Device into Database.")
                        {
                            StatusCode = 500
                        };
                    }
                }
                else
                {
                    Logger.LogInformation("DeviceController : AddDevice : Update operation device object NULL. Bad Request.");
                    actionResult = BadRequest();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"DeviceController : AddDevice : Error occured. Error Message : {ex.Message}, Stack Trace : {ex.StackTrace}");
                throw;
            }
            finally
            {
                Logger.LogInformation("DeviceController : AddDevice : Execution End.");
            }
            return actionResult;
        }

        [HttpPost("edit", Name = "UpdateDevice")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateDevice(DeviceEditDTO deviceEditDTO)
        {
            IActionResult actionResult;
            try
            {
                Logger.LogInformation("DeviceController : UpdateDevice : Execution Begin.");
                if (deviceEditDTO != null)
                {
                    Device device = Repository.GetAll()
                                        .Include(p => p.DeviceBackends)
                                            .FirstOrDefault(o => o.DeviceID == deviceEditDTO.DeviceID);

                    if (device != null)
                    {
                        Logger.LogInformation("DeviceController : UpdateDevice : Item found from Database.");

                        device.Model = deviceEditDTO.Model;
                        device.SimCardNumber = deviceEditDTO.SimCardNumber;
                        device.Enabled = deviceEditDTO.Enabled;

                        device.DeviceBackends.Clear();
                        if (deviceEditDTO.Backends != null)
                        {
                            deviceEditDTO.Backends.ForEach(backend =>
                            {
                                device.DeviceBackends.Add(new DeviceBackend { DeviceID = deviceEditDTO.DeviceID, BackendID = backend.BackendID });
                            });
                        }
                        Logger.LogInformation("DeviceController : UpdateDevice : Before calling Repository.Update().");
                        Repository.Update(device);
                        Logger.LogInformation("DeviceController : UpdateDevice : After calling Repository.Update().");

                        Logger.LogInformation("DeviceController : UpdateDevice : Before calling Commit in Database.");
                        UnitOfWork.Commit();
                        Logger.LogInformation("DeviceController : UpdateDevice : After calling Commit in Database.");

                        if (device.DeviceBackends?.Count > 0)
                        {
                            Logger.LogInformation($"DeviceController : UpdateDevice : Link to Device Backend found, link count : {device.DeviceBackends.Count}");
                            foreach (var deviceBackend in device.DeviceBackends)
                            {
                                // Prevent circular reference.
                                deviceBackend.Device = null;
                                if (deviceBackend.Backend != null)
                                    deviceBackend.Backend.DeviceBackends = null;
                            }
                        }

                        actionResult = Ok(device);
                        Logger.LogInformation("DeviceController : UpdateDevice : Ok.");
                    }
                    else
                    {
                        Logger.LogInformation("DeviceController : UpdateDevice : Item not found in Database.");
                        actionResult = NotFound($"Device with ID : {deviceEditDTO.DeviceID} not found in Database.");
                    }
                }
                else
                {
                    Logger.LogInformation("DeviceController : UpdateDevice : Bad Request.");
                    actionResult = BadRequest();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"DeviceController : UpdateDevice : Error occured. Error Message : {ex.Message}, Stack Trace : {ex.StackTrace}");
                throw;
            }
            finally
            {
                Logger.LogInformation("DeviceController : UpdateDevice : Execution End.");
            }
            return actionResult;
        }

        #endregion --- Actions ---
    }
}
