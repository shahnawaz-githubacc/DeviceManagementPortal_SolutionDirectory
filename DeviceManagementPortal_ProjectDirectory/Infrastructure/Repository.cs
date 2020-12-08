using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DeviceManagementPortal.Infrastructure.EF;
using DeviceManagementPortal.Infrastructure.Contracts;
using System.Linq.Expressions;

namespace DeviceManagementPortal.Infrastructure
{
    internal class Repository<T> : IRepository<T> where T : class
    {
        private DeviceManagementDbContext DeviceManagementDbContext = null;

        public Repository(IUnitOfWork unitOfWork)
        {
            DeviceManagementDbContext = unitOfWork.GetContext() as DeviceManagementDbContext;
        }

        public void Add(T item)
        {
            DeviceManagementDbContext.Entry<T>(item).State = EntityState.Added;
        }

        public void Delete(T item)
        {
            DeviceManagementDbContext.Entry<T>(item).State = EntityState.Deleted;
        }

        public async Task<T> Get(int id)
        {
            return await DeviceManagementDbContext.Set<T>().FindAsync(id);
        }

        public IQueryable<T> GetAll()
        {
            return DeviceManagementDbContext.Set<T>();
        }

        public void Update(T item)
        {
            DeviceManagementDbContext.Set<T>().Attach(item);
            DeviceManagementDbContext.Entry<T>(item).State = EntityState.Modified;
        }
    }
}
