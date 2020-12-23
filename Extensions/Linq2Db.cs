using LinqToDB;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace LINQ2DB_MVC_Core_5.Extensions
{
    public static class Linq2Db
	{
		/// <summary>
		///     Cpncurrency interface for <see cref="IIdentityRole{TKey}" /> and <see cref="IIdentityUser{TKey}" />/>
		/// </summary>
		/// <typeparam name="TKey">The type used for the primary key.</typeparam>
		public interface IConcurrency<TKey>
			where TKey : IEquatable<TKey>
		{
			/// <summary>
			///     Gets or sets the primary key.
			/// </summary>
			TKey Id { get; set; }

			/// <summary>
			///     A random value that should change whenever a role is persisted to the store
			/// </summary>
			string ConcurrencyStamp { get; set; }
		}

		public static int UpdateConcurrent<T, TKey>(this IDataContext dc, T obj)
			where T : class, IConcurrency<TKey>
			where TKey : IEquatable<TKey>
		{
			var stamp = Guid.NewGuid().ToString();

			var query = dc.GetTable<T>()
				.Where(_ => _.Id.Equals(obj.Id) && _.ConcurrencyStamp == obj.ConcurrencyStamp)
				.Set(_ => _.ConcurrencyStamp, stamp);

			var ed = dc.MappingSchema.GetEntityDescriptor(typeof(T));
			var p = Expression.Parameter(typeof(T));
			foreach (
				var column in
				ed.Columns.Where(
					_ => _.MemberName != nameof(IConcurrency<TKey>.ConcurrencyStamp) && !_.IsPrimaryKey && !_.SkipOnUpdate))
			{
				var expr = Expression
					.Lambda<Func<T, object>>(
						Expression.Convert(Expression.PropertyOrField(p, column.MemberName), typeof(object)),
						p);

				var val = column.MemberAccessor.Getter(obj);
				query = query.Set(expr, val);
			}

			var res = query.Update();
			obj.ConcurrencyStamp = stamp;

			return res;
		}
	}
}
