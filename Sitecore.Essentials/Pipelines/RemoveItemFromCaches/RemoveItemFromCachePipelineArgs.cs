namespace Sitecore.Essentials.Pipelines.RemoveItemFromCaches
{
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Pipelines;

  public class RemoveItemFromCachePipelineArgs : PipelineArgs
  {
    [NotNull]
    private readonly Item item;

    public RemoveItemFromCachePipelineArgs([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, "item");

      this.item = item;
    }

    [NotNull]
    public Item Item
    {
      get
      {
        return this.item;
      }
    }
  }
}