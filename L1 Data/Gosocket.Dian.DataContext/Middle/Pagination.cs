using System;
using System.Collections.Generic;
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

    public abstract class PagedResultBase
    {
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; }
        public int RowCount { get; set; }

        public int FirstRowOnPage => (CurrentPage - 1) * PageSize + 1;
        public int LastRowOnPage => Math.Min(CurrentPage * PageSize, RowCount);
    }

    public class PagedResult<TEntity> : PagedResultBase where TEntity : class
    {
        public List<TEntity> Results { get; set; }
        public PagedResult()
        {
            Results = new List<TEntity>();
        }
    }
}