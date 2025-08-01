using Ecommerce_APIs.Models.DTOs.GlobalFilterDtos;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;

namespace Ecommerce_APIs.Helpers.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyGlobalFilter<T>(
            this IQueryable<T> query,
            GlobalFilterDto filter,
            Expression<Func<T, string>>? searchSelector = null,
            Expression<Func<T, bool>>? isActiveSelector = null)
        {
            if (filter == null)
                return query;

            // 1️⃣ IsActive filtering
            if (isActiveSelector != null && filter.IsActive.HasValue)
            {
                query = query.Where(BuildIsActiveFilter(filter.IsActive.Value, isActiveSelector));
            }

            // 2️⃣ SearchTerm filtering
            if (searchSelector != null && !string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var propertyName = ((MemberExpression)searchSelector.Body).Member.Name;
                var searchTerm = filter.SearchTerm.Trim().ToLower();
                query = query.Where($"{propertyName}.ToLower().Contains(@0)", searchTerm);
            }

            // 3️⃣ Sorting
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                var sortFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "title", "Title" },
                { "createdat", "CreatedAt" },
                { "updatedat", "UpdatedAt" },
            };

                if (sortFields.TryGetValue(filter.SortBy, out var sortField))
                {
                    query = query.OrderBy($"{sortField} {(filter.Descending ? "descending" : "ascending")}");
                }
                else
                {
                    query = query.OrderBy("CreatedAt descending");
                }
            }

            return query;
        }

        // 🔧 Helper to avoid Expression.Invoke reuse issues
        private static Expression<Func<T, bool>> BuildIsActiveFilter<T>(bool isActive, Expression<Func<T, bool>> selector)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var body = Expression.Equal(
                Expression.Invoke(selector, parameter),
                Expression.Constant(isActive)
            );
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public static async Task<(List<T> Items, int TotalCount)> ToPagedListAsync<T>(
            this IQueryable<T> query,
            GlobalFilterDto filter)
        {
            var total = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
