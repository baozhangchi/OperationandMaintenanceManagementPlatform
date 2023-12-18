using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OAMMP.Models;
using SqlSugar;

namespace OAMMP.Common;

public abstract class RepositoryBase<T> : SimpleClient<T>, IDisposable where T : TableBase, new()
{
	public void Dispose()
	{
		Context?.Close();
		Context?.Dispose();
	}
}

public static class Repository
{
	static Repository()
	{
		ExternalServices = new ConfigureExternalServices
		{
			EntityService = (propertyInfo, entityColumnInfo) =>
			{
				if (entityColumnInfo.IsIgnore) return;

				if (entityColumnInfo.IsPrimarykey == false && new NullabilityInfoContext()
						.Create(propertyInfo).WriteState is NullabilityState.Nullable)
					entityColumnInfo.IsNullable = true;

				//最好排除DTO类
				if (entityColumnInfo.DbColumnName.Equals(propertyInfo.Name))
					entityColumnInfo.DbColumnName =
						UtilMethods.ToUnderLine(entityColumnInfo.DbColumnName); //ToUnderLine驼峰转下划线方法    
			},
			EntityNameService = (_, entityInfo) => //处理表名
			{
				//最好排除DTO类
				entityInfo.DbTableName = UtilMethods.ToUnderLine(entityInfo.DbTableName); //ToUnderLine驼峰转下划线方法
			}
		};
	}

	public static ConfigureExternalServices ExternalServices { get; }
}

public class Repository<T> : RepositoryBase<T> where T : TableBase, new()
{
	public Repository(ISqlSugarClient client)
	{
		Context = client;
	}

	public override async Task<bool> InsertAsync(T insertObj)
	{
		var id = await Context.Insertable(insertObj).ExecuteReturnSnowflakeIdAsync();
		return id > 0;
	}

	public override async Task<bool> InsertOrUpdateAsync(T data)
	{
		if (await IsAnyAsync(x => x.UUID == data.UUID)) return await UpdateAsync(data);

		return await InsertAsync(data);
	}

	public override async Task<bool> InsertRangeAsync(List<T> insertObjs)
	{
		var ids = await Context.Insertable(insertObjs).ExecuteReturnSnowflakeIdListAsync();
		return ids.Count > 0;
	}

	public override async Task<bool> UpdateAsync(T updateObj)
	{
		var num = await Context.Updateable(updateObj).WhereColumns(x => x.UUID).ExecuteCommandAsync();
		return num > 0;
	}
}

public class LogRepository<T> : Repository<T> where T : LogTableBase, new()
{
	private readonly Func<DateTime, ISqlSugarClient> _backupSqlClientFactory;

	public LogRepository(ISqlSugarClient client, Func<DateTime, ISqlSugarClient> backupSqlClientFactory) : base(client)
	{
		_backupSqlClientFactory = backupSqlClientFactory;
	}

	public async Task<T> GetLatestAsync(Expression<Func<T, bool>> whereExpression)
	{
		return await Context.Queryable<T>().OrderByDescending(x => x.Time).FirstAsync(whereExpression);
	}

	public async Task<List<T>> GetLatestListAsync(Expression<Func<T, bool>> whereExpression, int num)
	{
		var items = await Context.Queryable<T>().Where(whereExpression).OrderByDescending(x => x.Time).Take(num)
			.ToListAsync();
		items.Reverse();
		return items;
	}

	public async Task<List<T>> GetLatestListAsync(Expression<Func<T, bool>> whereExpression)
	{
		var items = await Context.Queryable<T>().Where(whereExpression).OrderByDescending(x => x.Time)
			.ToListAsync();
		items.Reverse();
		return items;
	}

	public async Task<List<T>> GetLatestListAsync(int num)
	{
		return await GetLatestListAsync(x => true, num);
	}

	public override async Task<bool> InsertRangeAsync(List<T> insertObjs)
	{
		if (await IsAnyAsync(x => x.Time < DateTime.Today.AddDays(-7)))
		{
			var items = await GetListAsync(x => x.Time < DateTime.Today.AddDays(-7));
			foreach (var data in items.GroupBy(x => x.Time.Date).Select(x => new { Date = x.Key, Items = x.ToList() }))
			{
				using (var backupRepository = new BackupRepository<T>(_backupSqlClientFactory(data.Date)))
				{
					await backupRepository.InsertRangeAsync(data.Items);
				}
			}
		}

		var ids = await Context.Insertable(insertObjs).ExecuteReturnSnowflakeIdListAsync();
		return ids.Count > 0;
	}


	public async Task<List<T>> GetLogData(QueryLogArgs args, Expression<Func<T, bool>>? exp = null)
	{
		var expression = new Expressionable<T>();
		if (exp != null)
		{
			expression.And(exp);
		}

		expression.AndIF(args.StartTime.HasValue, x => x.Time > args.StartTime!.Value);
		expression.AndIF(args.EndTime.HasValue, x => x.Time <= args.EndTime!.Value);
		if (args.Count.HasValue)
		{
			var items = await GetLatestListAsync(expression.ToExpression(), args.Count.Value);
			return items;
		}
		else
		{
			var items = await GetLatestListAsync(expression.ToExpression());
			return items;
		}
	}
}

public class BackupRepository<T> : Repository<T> where T : LogTableBase, new()
{
	public BackupRepository([FromKeyedServices("backup")] ISqlSugarClient client) : base(client)
	{
	}
}