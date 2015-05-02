namespace Sitecore.Essentials.Shell.Framework.Commands
{
  using System.Linq;
  using Sitecore;
  using Sitecore.Diagnostics;
  using Sitecore.Essentials.Pipelines.RemoveItemFromCaches;
  using Sitecore.Pipelines;
  using Sitecore.Shell.Framework.Commands;

  [UsedImplicitly]
  public class RemoveItemFromCaches : Command
  {
    public override void Execute([NotNull] CommandContext context)
    {
      Assert.ArgumentNotNull(context, "context");

      var items = context.Items;
      Assert.IsNotNull(items, "items");

      var item = items.FirstOrDefault();
      if (item != null)
      {
        Log.Audit(string.Format("Remove from caches: {0}", AuditFormatter.FormatItem(item)), this);
        Pipeline.Start("removeItemFromCaches", new RemoveItemFromCachePipelineArgs(item));
      }
    }
  }
}

