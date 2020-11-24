using Gosocket.Dian.Domain.Entity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.DataContext.Middle
{
    public static class PagedQuery
    {
        public static PagedResult<T> Paginate<T>(this IQueryable<T> query, int page, int pageSize, Expression<Func<T, string>> orderby) where T : class
        {
            var result = new PagedResult<T>
            {
                CurrentPage = page,
                PageSize = pageSize,
                RowCount = query.Count()
            };

            if (page == 0)
            {
                result.Results = query.ToList();
                return result;
            }
            query = query.OrderBy(orderby);

            double pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);

            int skip = (page - 1) * pageSize;
            IQueryable<T> sql = query.Skip(skip).Take(pageSize).AsQueryable();
            result.Results = sql.ToList();

            return result;
        }
    }
}