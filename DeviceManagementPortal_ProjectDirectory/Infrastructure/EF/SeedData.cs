using DeviceManagementPortal.Models.DomainModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManagementPortal.Infrastructure.EF
{
    public static class SeedData
    {
        public static void EnsurePopulated(IApplicationBuilder applicationBuilder)
        {
            var dbContext = applicationBuilder.ApplicationServices.CreateScope()
                        .ServiceProvider.GetRequiredService<DeviceManagementDbContext>();

            if (dbContext != null)
            {
                if (dbContext.Database.GetPendingMigrations().Any())
                {
                    dbContext.Database.Migrate();
                }

                SeedApplicationData(dbContext);
            }

            var dbIdentityContext = applicationBuilder.ApplicationServices.CreateScope()
                        .ServiceProvider.GetRequiredService<DeviceManagementIdentityDbContext>();

            if (dbIdentityContext != null)
            {
                if (dbIdentityContext.Database.GetPendingMigrations().Any())
                {
                    dbIdentityContext.Database.Migrate();
                }

                SeedIdentityData(applicationBuilder, dbIdentityContext);
            }
        }

        private static async void SeedIdentityData(IApplicationBuilder applicationBuilder, DeviceManagementIdentityDbContext dbIdentityContext)
        {
            const string userName = "admin";
            const string password = "admin@123";

            UserManager<IdentityUser> userManager = applicationBuilder.ApplicationServices
                                                        .CreateScope().ServiceProvider
                                                            .GetRequiredService<UserManager<IdentityUser>>();

            IdentityUser user = await userManager.FindByIdAsync(userName);
            if (user == null)
            {
                user = new IdentityUser(userName);
                user.Email = "admin@example.com";
                await userManager.CreateAsync(user, password);
            }
        }

        private static void SeedApplicationData(DeviceManagementDbContext dbContext)
        {
            if (!dbContext.Backends.Any())
            {
                string backendName = string.Empty;
                string addressName = string.Empty;
                for (int index = 1; index <= 10; index++)
                {
                    backendName = $"Backend {index}";
                    addressName = $"Address {index}";
                    dbContext.Backends.Add(SeedBackend(backendName, addressName));
                }
                backendName = "Name with: 20 chars.";
                addressName = "Large address to check 50 chars limit in colm val.";
                dbContext.Backends.Add(SeedBackend(backendName, addressName));
            }
            dbContext.SaveChanges();

            if (!dbContext.Devices.Any())
            {
                string imei = string.Empty;
                string model = string.Empty;
                bool enabled = false;
                long simCardNumber = 0;
                for (int index = 1; index <= 10; index++)
                {
                    imei = $"IMEI: {12345678 + index}";
                    model = $"Model-{index}";
                    enabled = (index % 2) == 0;
                    simCardNumber = long.Parse("123456789" + index.ToString());
                    dbContext.Devices.Add(SeedDevice(imei, model, enabled, simCardNumber, "admin", DateTime.Now));
                }
            }
            dbContext.SaveChanges();

            if (!dbContext.DeviceBackends.Any())
            {
                dbContext.DeviceBackends.Add(SeedDeviceBackend(1, 1));
                dbContext.DeviceBackends.Add(SeedDeviceBackend(4, 1));
                dbContext.DeviceBackends.Add(SeedDeviceBackend(5, 1));
                dbContext.DeviceBackends.Add(SeedDeviceBackend(6, 1));
                dbContext.DeviceBackends.Add(SeedDeviceBackend(1, 2));
                dbContext.DeviceBackends.Add(SeedDeviceBackend(2, 5));
                dbContext.DeviceBackends.Add(SeedDeviceBackend(2, 6));
                dbContext.DeviceBackends.Add(SeedDeviceBackend(2, 7));
                dbContext.DeviceBackends.Add(SeedDeviceBackend(9, 3));
                dbContext.DeviceBackends.Add(SeedDeviceBackend(7, 3));
            }
            dbContext.SaveChanges();
        }

        private static Backend SeedBackend(string name, string address) => new Backend { Name = name, Address = address };

        private static Device SeedDevice(string imei, string model, bool enabled, long simCardNumber, string createdBy, DateTime createdDateTime)
            => new Device { IMEI = imei, Model = model, Enabled = enabled, SimCardNumber = simCardNumber, CreatedBy = createdBy, CreatedDateTime = createdDateTime };

        private static DeviceBackend SeedDeviceBackend(int backendId, int deviceId) => new DeviceBackend { BackendID = backendId, DeviceID = deviceId };
    }
}
