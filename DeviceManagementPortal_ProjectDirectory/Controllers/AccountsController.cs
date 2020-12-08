using DeviceManagementPortal.Models.DomainModels;
using DeviceManagementPortal.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManagementPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        #region --- Global Variables ---

        private readonly SignInManager<IdentityUser> SignInManager;
        private readonly UserManager<IdentityUser> UserManager;
        private readonly IConfiguration Configuration;
        private readonly ILogger<AccountsController> Logger;

        #endregion --- Global Variables ---

        #region --- Constructor ---

        public AccountsController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IConfiguration configuration, ILogger<AccountsController> logger)
        {
            SignInManager = signInManager;
            UserManager = userManager;
            Configuration = configuration;
            Logger = logger;

            Logger.LogInformation("AccountsController : Parameterized Constructor Execution Finished.");
        }

        #endregion --- Constructor ---

        #region --- Actions ---

        [HttpPost("token")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GenerateJWTToken([FromBody] IdentityUserDTO credential)
        {
            Logger.LogInformation("AccountsController : GenerateJWTToken : GenerateJWTToken Start.");
            if (await CheckPasswordValidators(credential))
            {
                if (await CheckCredentials(credential))
                {
                    JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
                    Logger.LogInformation("AccountsController : GenerateJWTToken : 'JwtSecurityTokenHandler' instance created.");

                    Logger.LogInformation("AccountsController : GenerateJWTToken : Before reading 'JWTSecret' key.");
                    string jwtSecret = Configuration.ReadConfigurationKeyValue("JWTSecret", string.Empty);
                    Logger.LogInformation("AccountsController : GenerateJWTToken : After reading 'JWTSecret' key.");

                    if (!string.IsNullOrEmpty(jwtSecret))
                    {
                        byte[] secret = Encoding.ASCII.GetBytes(jwtSecret);
                        Logger.LogInformation("AccountsController : GenerateJWTToken : 'JWTSecret' value read.");
                        SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
                        {
                            Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, credential.UserName) }),
                            Expires = DateTime.UtcNow.AddHours(1),
                            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256Signature)
                        };
                        Logger.LogInformation("AccountsController : GenerateJWTToken : 'SecurityTokenDescriptor' instance created.");

                        Logger.LogInformation("AccountsController : GenerateJWTToken : Before calling handler.CreateToken().");
                        SecurityToken token = jwtSecurityTokenHandler.CreateToken(descriptor);
                        Logger.LogInformation("AccountsController : GenerateJWTToken : After calling handler.CreateToken().");

                        Logger.LogInformation("AccountsController : GenerateJWTToken : Execution Finish: Successful.");
                        return Ok(new
                        {
                            success = true,
                            token = jwtSecurityTokenHandler.WriteToken(token)
                        });
                    }
                    else
                    {
                        Logger.LogError("AccountsController : GenerateJWTToken : 'JWTToken' missing.");
                        throw new Exception("Authentication token missing.");
                    }
                }
                else
                {
                    Logger.LogInformation("AccountsController : GenerateJWTToken : Password check failed.");
                }
            }
            else
            {
                Logger.LogInformation("AccountsController : GenerateJWTToken : Password validators check failed.");
            }
            Logger.LogInformation("AccountsController : GenerateJWTToken : Execution Finish: Unsuccessful.");
            return Unauthorized();
        }

        [HttpGet("user")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUserDetails()
        {
            Logger.LogInformation("AccountsController : GenerateJWTToken : GetUserDetails Start.");
            Logger.LogDebug($"AccountsController : GenerateJWTToken : Authenticated user name : {HttpContext.User.Identity.Name}.");
            Logger.LogInformation("AccountsController : GenerateJWTToken : GetUserDetails Finish.");
            return Ok(HttpContext.User.Identity.Name);
        }

        #endregion --- Actions ---

        #region --- Private Members ---

        private async Task<bool> CheckPasswordValidators(IdentityUserDTO credential)
        {
            Logger.LogInformation("AccountsController : CheckPassword : CheckPassword Start.");
            Logger.LogInformation("AccountsController : CheckPassword : Before calling UserManager.FindByNameAsync().");
            IdentityUser user = await UserManager.FindByNameAsync(credential.UserName);
            Logger.LogInformation("AccountsController : CheckPassword : After calling UserManager.FindByNameAsync().");

            if (user != null)
            {
                foreach (IPasswordValidator<IdentityUser> passwordValidator in UserManager.PasswordValidators)
                {
                    if ((await passwordValidator.ValidateAsync(UserManager, user, credential.Password)).Succeeded)
                    {
                        Logger.LogInformation("AccountsController : CheckPassword : CheckPassword Finish : Successful.");
                        return true;
                    }
                }
            }
            else
            {
                Logger.LogInformation("AccountsController : CheckPassword : 'user' is NULL.");
            }
            Logger.LogInformation("AccountsController : GenerateJWTToken : CheckPassword Finish : Unsuccessful.");
            return false;
        }

        private async Task<bool> CheckCredentials(IdentityUserDTO credential)
        {
            IdentityUser user = await UserManager.FindByNameAsync(credential.UserName);
            var signInResult = await SignInManager.CheckPasswordSignInAsync(user, credential.Password, false);
            if (signInResult.Succeeded)
            {
                return true;
            }
            return false;
        }

        #endregion --- Private Members ---
    }
}
