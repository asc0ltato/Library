using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace WCFService.Repository
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        IQueryable<T> Include(params Expression<Func<T, object>>[] includeProperties);
        void Add(T entity);
        void Update(T entity);
        void Remove(int id);
        void Remove(T entity);
    }
}