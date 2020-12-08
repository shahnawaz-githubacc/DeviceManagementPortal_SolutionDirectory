using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeviceManagementPortal.Infrastructure.Contracts;
using DeviceManagementPortal.Models.DomainModels;
using DeviceManagementPortal.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeviceManagementPortal.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class BackendsController : ControllerBase
    {
        #region --- Global Variables ---

        private readonly IUnitOfWork UnitOfWork = null;
        private readonly IRepository<Backend> Repository = null;
        private ILogger<BackendsController> Logger = null;

        #endregion --- Global Variables ---

        #region --- Constructor ---

        public BackendsController(IUnitOfWork unitOfWork, IRepository<Backend> repository, ILogger<BackendsController> logger)
        {
            UnitOfWork = unitOfWork;
            Repository = repository;
            Logger = logger;

            Logger.LogInformation("BackendsController : Parameterized Constructor Execution Finished.");
        }

        #endregion --- Constructor ---

        #region --- Actions ---

        [HttpGet("all", Name = "GetAllBackends")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAllBackends()
        {
            IActionResult actionResult;
            Logger.LogInformation("BackendsController : GetAllDevices : Execution Start.");
            try
            {
                Logger.LogInformation("BackendsController : GetAllBackends : Before invoking Repository.GetAll().");
                IQueryable<Backend> backends = Repository.GetAll().OrderBy(b => b.BackendID);
                Logger.LogInformation("BackendsController : GetAllBackends : After invoking Repository.GetAll().");

                int totalItemCount = backends.Count();
                Logger.LogDebug($"BackendsController : GetAllBackends : Total item count : {totalItemCount}");
                if (totalItemCount > 0)
                {
                    Logger.LogDebug($"BackendsController : GetAllBackends : Backends found in DB : {totalItemCount}");
                    actionResult = Ok(backends);
                }
                else
                {
                    Logger.LogInformation($"BackendsController : GetAllBackends : No devices found in DB.");
                    actionResult = Ok("No backends found in Database.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"DeviceController : GetAllBackends : Error occured. Error Message : {ex.Message}, Stack Trace : {ex.StackTrace}");
                throw;
            }
            finally
            {
                Logger.LogInformation("DeviceController : GetAllBackends : Execution Finished.");
            }
            return actionResult;
        }

        [HttpGet("get/{id:int}", Name = "GetBackend")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBackend(int id)
        {
            IActionResult actionResult;
            Logger.LogInformation("BackendsController : GetBackend : Execution Start.");
            Logger.LogDebug($"BackendsController : GetBackend : Parameter 'id', Value : {id}");
            try
            {
                Logger.LogInformation("BackendsController : GetBackend : Before invoking Repository.GetAll().");
                Backend backend = await Repository.Get(id);              
                Logger.LogInformation("BackendsController : GetBackend : After invoking Repository.GetAll().");

                if (backend != null)
                {
                    Logger.LogInformation("BackendsController : GetBackend : HTTP 200.");
                    actionResult = Ok(backend);
                }
                else
                {
                    Logger.LogInformation("BackendsController : GetBackend : Item not found in Database.");
                    actionResult = NotFound($"Backend with ID : {id} not found in Database.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"BackendsController : GetBackend : Error occured. Error Message : {ex.Message}, Stack Trace : {ex.StackTrace}");
                throw;
            }
            finally
            {
                Logger.LogInformation("BackendsController : GetBackend : Execution Finished.");
            }
            return actionResult;
        }

        [HttpDelete("delete/{id:int}", Name = "DeleteBackend")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBackend(int id)
        {
            IActionResult actionResult;
            Logger.LogInformation("BackendsController : DeleteBackend : Execution Start.");
            Logger.LogDebug($"BackendsController : DeleteBackend : Parameter 'id', Value : {id}");
            try
            {
                Logger.LogInformation("BackendsController : DeleteBackend : Before invoking Repository.Get().");
                Backend backend = await Repository.Get(id);
                Logger.LogInformation("BackendsController : DeleteBackend : After invoking Repository.Get().");

                if (backend != null)
                {
                    Logger.LogInformation("BackendsController : DeleteBackend : Before invoking Repository.Delete().");
                    Repository.Delete(backend);
                    Logger.LogInformation("BackendsController : DeleteBackend : After invoking Repository.Delete().");

                    Logger.LogInformation("BackendsController : DeleteBackend : Before invoking UnitOfWork.Commit().");
                    UnitOfWork.Commit();
                    Logger.LogInformation("BackendsController : DeleteBackend : After invoking UnitOfWork.Commit().");

                    actionResult = Ok($"Backend with ID : {id} deleted from Database.");
                }
                else
                {
                    Logger.LogDebug($"BackendsController : DeleteBackend : Item with DeviceID : {id} not found.");
                    actionResult = NotFound($"Backend with ID : {id} not found in Database.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"BackendsController : DeleteBackend : Error occured. Error Message : {ex.Message}, Stack Trace : {ex.StackTrace}");
                throw;
            }
            finally
            {
                Logger.LogInformation("BackendsController : DeleteBackend : Execution End.");
            }
            return actionResult;
        }

        [HttpPost("add", Name = "AddBackend")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddBackend(BackendApiDTO backendApiDTO)
        {
            IActionResult actionResult;
            try
            {
                Logger.LogInformation("BackendsController : AddBackend : Execution Begin.");
                if (backendApiDTO != null)
                {
                    Backend backend = Backend.MapApi(backendApiDTO);

                    Logger.LogInformation("BackendsController : AddBackend : Before invoking Repository.Add().");
                    Repository.Add(backend);
                    Logger.LogInformation("BackendsController : AddBackend : After invoking Repository.Add().");

                    Logger.LogInformation("BackendsController : AddBackend : Before committing to Database.");
                    UnitOfWork.Commit();
                    Logger.LogInformation("BackendsController : AddBackend : After committing to Database.");

                    if (backend.BackendID > 0)
                    {
                        Logger.LogDebug($"BackendsController : AddBackend : New Backend ID : {backend.BackendID}.");
                        actionResult = Ok(backend);
                        Logger.LogInformation("BackendsController : AddBackend : HTTP 200.");
                    }
                    else
                    {
                        actionResult = new ObjectResult("Error occured while saving Backend into Database.")
                        {
                            StatusCode = 500
                        };
                    }
                }
                else
                {
                    Logger.LogInformation("BackendsController : AddBackend : Update operation device object NULL. Bad Request.");
                    actionResult = BadRequest();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"BackendsController : AddBackend : Error occured. Error Message : {ex.Message}, Stack Trace : {ex.StackTrace}");
                throw;
            }
            finally
            {
                Logger.LogInformation("BackendsController : AddBackend : Execution End.");
            }
            return actionResult;
        }

        [HttpPut("edit", Name = "UpdateBackend")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateBackend(BackendApiDTO backendApiDTO)
        {
            IActionResult actionResult;
            try
            {
                Logger.LogInformation("BackendsController : UpdateBackend : Execution Begin.");
                if (backendApiDTO != null)
                {
                    Logger.LogDebug($"BackendsController : UpdateBackend : Incoming Backend ID : {backendApiDTO.BackendID}.");
                    Backend backend = Repository.GetAll()
                                            .FirstOrDefault(o => o.BackendID == backendApiDTO.BackendID);

                    if (backend != null)
                    {
                        Logger.LogInformation("BackendsController : UpdateBackend : Item found from Database.");

                        backend.Address = backendApiDTO.Address;
                        backend.Name = backendApiDTO.Name;

                        Logger.LogInformation("BackendsController : UpdateBackend : Before calling Repository.Update().");
                        Repository.Update(backend);
                        Logger.LogInformation("BackendsController : UpdateBackend : After calling Repository.Update().");

                        Logger.LogInformation("BackendsController : UpdateBackend : Before calling Commit in Database.");
                        UnitOfWork.Commit();
                        Logger.LogInformation("BackendsController : UpdateBackend : After calling Commit in Database.");

                        actionResult = Ok(backend);
                        Logger.LogInformation("BackendsController : UpdateBackend : Ok.");
                    }
                    else
                    {
                        Logger.LogInformation("BackendsController : UpdateBackend : Item not found in Database.");
                        actionResult = NotFound($"Device with ID : {backendApiDTO.BackendID} not found from Database.");
                    }
                }
                else
                {
                    Logger.LogInformation("BackendsController : UpdateBackend : Bad Request.");
                    actionResult = BadRequest();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"BackendsController : UpdateBackend : Error occured. Error Message : {ex.Message}, Stack Trace : {ex.StackTrace}");
                throw;
            }
            finally
            {
                Logger.LogInformation("BackendsController : UpdateBackend : Execution End.");
            }
            return actionResult;
        }

        #endregion --- Actions ---
    }
}
