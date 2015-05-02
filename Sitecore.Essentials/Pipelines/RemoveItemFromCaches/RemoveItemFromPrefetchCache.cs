namespace Sitecore.Essentials.Pipelines.RemoveItemFromCaches
{
  using System.Reflection;
  using Sitecore.Data;
  using Sitecore.Data.DataProviders.Sql;
  using Sitecore.Diagnostics;

  public class RemoveItemFromPrefetchCache
  {
    [UsedImplicitly]
    public void Process([NotNull] RemoveItemFromCachePipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      var item = args.Item;

      var database = item.Database;
      Assert.IsNotNull(database, "database");

      var dataProviders = database.GetDataProviders();
      Assert.IsNotNull(dataProviders, "dataProviders");

      var sqlDataProvider = dataProviders[0] as SqlDataProvider;
      Assert.IsNotNull(sqlDataProvider, "sqlDataProvider");

      var type = typeof(SqlDataProvider);
      var flags = BindingFlags.NonPublic | BindingFlags.Instance;
      var types = new[] { typeof(ID) };
      var method = type.GetMethod("RemovePrefetchDataFromCache", flags, null, types, null);
      Assert.IsNotNull(method, "method");

      method.Invoke(sqlDataProvider, new[] { item.ID as object });
    }
  }
}