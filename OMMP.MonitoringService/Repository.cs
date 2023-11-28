using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using OMMP.Models;
using SqlSugar;

namespace OMMP.MonitoringService;

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

        var ids = await Context.Insertable(insertObjs).ExecuteReturnSnowflakeIdListAsync();
        return ids.Count > 0;
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

    public async Task<T> GetLatestAsync(Expression<Func<T, bool>> whereExpression)
    {
        return await Context.Queryable<T>().OrderByDescending(x => x.Time).FirstAsync(whereExpression);
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
        var regex = new Regex(@"(?<from>\d{8})-(?<to>\d{8}).db");
        var dbFiles = new DirectoryInfo(GlobalCache.DataFolder).GetFiles("*.db.bak")
            .OrderByDescending(x => x.CreationTime).ToList();
        var dbFileMap = dbFiles
            .Select(x =>
            {
                var match = regex.Match(x.Name);
                var from = DateTime.ParseExact(match.Groups["from"].Value, @"yyyyMMdd", CultureInfo.InvariantCulture);
                var to = DateTime.ParseExact(match.Groups["to"].Value, @"yyyyMMdd", CultureInfo.InvariantCulture);
                return new { File = x, From = from, To = to };
            }).ToList();
        var dbFile = dbFileMap.FirstOrDefault(x => x.From <= day.Date && x.To > day.Date)?.File;
        if (dbFile == null)
            dbFile = new FileInfo(Path.Combine(GlobalCache.DataFolder,
                $"{day:yyyyMMdd}-{day.AddDays(7):yyyyMMdd}.db.bak"));

        var client = new SqlSugarClient(
            new ConnectionConfig()
            {
                DbType = DbType.Sqlite,
                ConnectionString = $"DataSource={dbFile.FullName}",
                IsAutoCloseConnection = true,
                ConfigureExternalServices = RepositoryBase.ExternalServices
            }, db =>
            {
                db.Aop.OnError = (exception) =>
                {
                    var sql = UtilMethods.GetNativeSql(exception.Sql, (SugarParameter[])exception.Parametres);
                    Console.WriteLine(sql);
                };
            });
        client.DbMaintenance.CreateDatabase();
        client.CodeFirst.InitTables(typeof(LogTableBase).Assembly.GetTypes()
            .Where(x => !x.IsAbstract && typeof(LogTableBase).IsAssignableFrom(x)).ToArray());
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

    public override async Task<bool> InsertRangeAsync(List<T> insertObjs)
    {
        return (await Context.Insertable(insertObjs).ExecuteReturnSnowflakeIdListAsync()).Count > 0;
    }
}