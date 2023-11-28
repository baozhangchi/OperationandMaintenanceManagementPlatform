using System.Reflection;
using SqlSugar;

namespace OMMP.Models;

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

    public override async Task<bool> InsertOrUpdateAsync(T data)
    {
        if (await IsAnyAsync(x => x.UUID == data.UUID)) return await UpdateAsync(data);

        return await InsertAsync(data);
    }

    public override async Task<bool> InsertAsync(T insertObj)
    {
        var id = await Context.Insertable(insertObj).ExecuteReturnSnowflakeIdAsync();
        return id > 0;
    }

    public override async Task<bool> UpdateAsync(T updateObj)
    {
        var num = await Context.Updateable(updateObj).WhereColumns(x => x.UUID).ExecuteCommandAsync();
        return num > 0;
    }
}

public static class RepositoryBase
{
    public static ConfigureExternalServices ExternalServices { get; }

    static RepositoryBase()
    {
        ExternalServices = new ConfigureExternalServices()
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

    public static ISqlSugarClient GetClient(string dataSource)
    {
        var client = new SqlSugarClient(new ConnectionConfig()
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dataSource}",
            IsAutoCloseConnection = true,
            ConfigureExternalServices = ExternalServices
        }, db =>
        {
            db.Aop.OnError = (exception) =>
            {
                var sql = UtilMethods.GetNativeSql(exception.Sql, (SugarParameter[])exception.Parametres);
                Console.WriteLine(sql);
            };
        });
        client.DbMaintenance.CreateDatabase();
        client.CodeFirst.InitTables(typeof(TableBase).Assembly.GetTypes()
            .Where(x => !x.IsAbstract && typeof(TableBase).IsAssignableFrom(x)).ToArray());
        return client;
    }

    public static ISqlSugarClient GetClient(string dataSource, params Type[] tableTypes)
    {
        var client = new SqlSugarClient(new ConnectionConfig()
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dataSource}",
            IsAutoCloseConnection = true,
            ConfigureExternalServices = ExternalServices
        }, db =>
        {
            db.Aop.OnError = (exception) =>
            {
                var sql = UtilMethods.GetNativeSql(exception.Sql, (SugarParameter[])exception.Parametres);
                Console.WriteLine(sql);
            };
        });
        client.DbMaintenance.CreateDatabase();
        if (tableTypes.Length > 0)
        {
            client.CodeFirst.InitTables(tableTypes);
        }
        return client;
    }
}