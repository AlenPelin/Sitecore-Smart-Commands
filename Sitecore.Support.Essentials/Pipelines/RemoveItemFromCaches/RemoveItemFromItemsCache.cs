namespace Sitecore.Support.Pipelines.RemoveItemFromCaches
{
  using Sitecore.Caching;
  using Sitecore.Diagnostics;

  public class RemoveItemFromItemsCache
  {
    [UsedImplicitly]
    public void Process([NotNull] RemoveItemFromCachePipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      var item = args.Item;

      var cache = CacheManager.GetItemCache(item.Database);
      Assert.IsNotNull(cache, "cache");

      cache.RemoveItem(item.ID);
    }
  }
}
