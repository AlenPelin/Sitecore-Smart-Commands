namespace Sitecore.Support.Pipelines.RemoveItemFromCaches
{
  using Sitecore.Caching;
  using Sitecore.Diagnostics;

  public class RemoveItemFromDataCache
  {
    [UsedImplicitly]
    public void Process([NotNull] RemoveItemFromCachePipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      var item = args.Item;

      var cache = CacheManager.GetDataCache(item.Database);
      Assert.IsNotNull(cache, "cache");

      cache.RemoveItemInformation(item.ID);
    }
  }
}