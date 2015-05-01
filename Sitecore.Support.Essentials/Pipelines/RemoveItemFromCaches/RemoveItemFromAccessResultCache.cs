namespace Sitecore.Support.Pipelines.RemoveItemFromCaches
{
  using System.Reflection;
  using Sitecore.Caching;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;

  public class RemoveItemFromAccessResultCache
  {
    [UsedImplicitly]
    public void Process([NotNull] RemoveItemFromCachePipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      var cache = CacheManager.GetAccessResultCache();
      Assert.IsNotNull(cache, "cache");

      var flags = BindingFlags.Instance | BindingFlags.NonPublic;
      var types = new[] { typeof(Item) };
      var method = typeof(AccessResultCache).GetMethod("RemoveItem", flags, null, types, null);
      Assert.IsNotNull(method, "method");

      foreach (var version in args.Item.Versions.GetVersions(true))
      {
        method.Invoke(cache, new[] { version as object });
      }
    }
  }
}