using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using OMMP.Models;
using SqlSugar;

namespace OMMP.MonitoringService;

public static class RepositoryBase
{
    public static ConfigureExternalServices ExternalServices { get; }

    static RepositoryBase()
    {
        ExternalServices = new ConfigureExternalServices()
        {
            EntityService = (propertyInfo, entityColumnInfo) =>
            {
                if (entityColumnInfo.IsPrimarykey == false && new NullabilityInfoContext()
                        .Create(propertyInfo).WriteState is NullabilityState.Nullable)
                {
                    entityColumnInfo.IsNullable = true;
                }

                //最好排除DTO类
                if (!entityColumnInfo.DbColumnName.Equals(propertyInfo.Name))
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

    public static ISqlSugarClient GetClient()
    {
        var client = new SqlSugarClient(new ConnectionConfig()
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={Path.Combine(GlobalCache.DataFolder, $"latest.db")}",
            IsAutoCloseConnection = true,
            ConfigureExternalServices = RepositoryBase.ExternalServices
        });
        client.DbMaintenance.CreateDatabase();
        client.CodeFirst.InitTables(typeof(TableBase).Assembly.GetTypes()
            .Where(x => !x.IsAbstract && typeof(TableBase).IsAssignableFrom(x)).ToArray());
        return client;
    }
}
public abstract class RepositoryBase<T> : SimpleClient<T>, IDisposable where T : TableBase, new()
{
    public void Dispose()
    {
        
    }
}
public class Repository<T> : RepositoryBase<T> where T : TableBase, new()
{
    public Repository(ISqlSugarClient client)
    {
        Context = client;
    }
}

public class LogRepository<T> : Repository<T> where T : LogTableBase, new()
{
    public override async Task<bool> InsertRangeAsync(List<T> insertObjs)
    {
        if (await IsAnyAsync(x => x.Time < DateTime.Today.AddDays(-7)))
        {
            var items = await GetListAsync(x => x.Time < DateTime.Today.AddDays(-7));
            foreach (var data in items.GroupBy(x => x.Time.Date).Select(x => new { Date = x.Key, Items = x.ToList() }))
            {
                var client = BackupRepository.GetClient(data.Date);
                using (var repository = BackupRepository<T>.CreateInstance(client))
                {
                    await repository.InsertRangeAsync(data.Items);
                }
            }
        }

        return await base.InsertRangeAsync(insertObjs);
    }

    public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> whereExpression, int num)
    {
        return await Context.Queryable<T>().OrderByDescending(x => x.Time).Where(whereExpression).Take(num)
            .ToListAsync();
    }

    public async Task<List<T>> GetListAsync(int num)
    {
        return await Context.Queryable<T>().OrderByDescending(x => x.Time).Take(num).ToListAsync();
    }

    public LogRepository(ISqlSugarClient client) : base(client)
    {
    }

    public static LogRepository<T> CreateInstance(ISqlSugarClient client)
    {
        return new LogRepository<T>(client);
    }
}

public static class BackupRepository
{
    public static ISqlSugarClient GetClient(DateTime day)
    {
        var regex = new Regex(@"(?<from>\d{8})-(?<to>]d{8}).db");
        var dbFiles = new DirectoryInfo(GlobalCache.DataFolder).GetFiles("*.db").OrderByDescending(x => x.CreationTime)
            .Select(x =>
            {
                var match = regex.Match(x.Name);
                var from = DateTime.ParseExact(match.Groups["from"].Value, @"yyyyMMdd", CultureInfo.CurrentCulture);
                var to = DateTime.ParseExact(match.Groups["to"].Value, @"yyyyMMdd", CultureInfo.CurrentCulture);
                return new { File = x, From = from, To = to };
            }).ToList();
        var dbFile = dbFiles.FirstOrDefault(x => x.From <= day.Date && x.To > day.Date)?.File.FullName;
        if (string.IsNullOrWhiteSpace(dbFile))
        {
            dbFile = Path.Combine(GlobalCache.DataFolder, $"{day:yyyyMMdd}-{day.AddDays(7):yyyyMMdd}.db");
        }

        var client = new SqlSugarClient(
            new ConnectionConfig()
            {
                DbType = DbType.Sqlite,
                ConnectionString = $"DataSource={dbFile}",
                IsAutoCloseConnection = true,
                ConfigureExternalServices = RepositoryBase.ExternalServices
            });
        client.DbMaintenance.CreateDatabase();
        client.CodeFirst.InitTables(typeof(TableBase).Assembly.GetTypes()
            .Where(x => !x.IsAbstract && typeof(TableBase).IsAssignableFrom(x)).ToArray());
        return client;
    }
}

public class BackupRepository<T> : Repository<T> where T : TableBase, new()
{
    private BackupRepository(ISqlSugarClient client) : base(client)
    {
    }

    public static BackupRepository<T> CreateInstance(ISqlSugarClient client)
    {
        return new BackupRepository<T>(client);
    }
}