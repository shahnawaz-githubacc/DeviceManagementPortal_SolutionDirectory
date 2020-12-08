using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DeviceManagementPortal.Infrastructure.Contracts
{
    public interface IRepository<T>
    {
        IQueryable<T> GetAll();

        Task<T> Get(int id);

        void Add(T item);

        void Update(T item);

        void Delete(T item);
    }
}
