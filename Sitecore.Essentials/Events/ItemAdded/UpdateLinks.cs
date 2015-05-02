namespace Sitecore.Essentials.Events.ItemAdded
{
  using System;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Essentials.Jobs;
  using Sitecore.Events;
  using Sitecore.StringExtensions;

  public class UpdateLinks
  {
    private readonly bool isAsync;

    public UpdateLinks()
    {
      this.isAsync = false;
    }

    public UpdateLinks([NotNull] string async)
    {
      Assert.ArgumentNotNull(async, "async");

      this.isAsync = string.Equals(async, "true", StringComparison.OrdinalIgnoreCase);
    }

    [UsedImplicitly]
    public void OnItemAdded([CanBeNull] object sender, [NotNull] EventArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      var contentItem = Event.ExtractParameter(args, 0) as Item;
      Assert.IsNotNull(contentItem, "targetItem");

      var branchItem = contentItem.Branch;
      if (branchItem == null)
      {
        return;
      }

      var item = branchItem.InnerItem;
      Assert.IsTrue(item.Children.Count == 1, "branch item structure is corrupted: {0}".FormatWith(AuditFormatter.FormatItem(item)));


      var branch = item.Children[0];
      if (this.isAsync)
      {
        ReferenceReplacementJob.StartAsync(branch, contentItem);
      }
      else
      {
        ReferenceReplacementJob.Start(branch, contentItem);
      }
    }
  }
}
