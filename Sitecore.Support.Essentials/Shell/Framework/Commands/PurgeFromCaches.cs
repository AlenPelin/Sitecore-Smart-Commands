namespace Sitecore.Support.Shell.Framework.Commands
{
  using System.Linq;
  using Sitecore.Diagnostics;
  using Sitecore.Pipelines;
  using Sitecore.Shell.Framework.Commands;
  using Sitecore.Support.Pipelines.RemoveItemFromCaches;

  [UsedImplicitly]
  public class PurgeFromCaches : Command
  {
    public override void Execute([NotNull] CommandContext context)
    {
      Assert.ArgumentNotNull(context, "context");

      var items = context.Items;
      Assert.IsNotNull(items, "items");

      var item = items.FirstOrDefault();
      if (item != null)
      {
        Log.Audit(string.Format("Purging {0} item from cache", item.ID), this);
        Pipeline.Start("removeItemFromCache", new RemoveItemFromCachePipelineArgs(item));
      }
    }
  }
}
