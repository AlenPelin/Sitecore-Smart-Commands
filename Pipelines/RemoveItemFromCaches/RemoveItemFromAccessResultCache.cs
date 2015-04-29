namespace Sitecore.Support.Pipelines.RemoveItemFromCaches
{
  using Sitecore.Caching;
  using Sitecore.Diagnostics;

  public class RemoveItemFromAccessResultCache
  {
    [UsedImplicitly]
    public void Process([NotNull] RemoveItemFromCachePipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      var item = args.Item;

      var cache = CacheManager.GetAccessResultCache();
      Assert.IsNotNull(cache, "cache");

      var id = item.ID;
      Assert.IsNotNull(id, "id");

      cache.RemoveKeysContaining(id.ToString());
    }
  }
}