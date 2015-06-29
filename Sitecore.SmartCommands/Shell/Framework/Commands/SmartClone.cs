namespace Sitecore.SmartCommands.Shell.Framework.Commands
{
  using System.Collections.Specialized;
  using Sitecore;
  using Sitecore.Configuration;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Shell.Framework.Commands;
  using Sitecore.Shell.Framework.Pipelines;
  using Sitecore.Text;

  [UsedImplicitly]
  public class SmartClone : Clone
  {
    public override void Execute([NotNull] CommandContext context)
    {
      Assert.ArgumentNotNull(context, "context");

      var items = context.Items;
      Assert.IsNotNull(items, "items");

      if (!Settings.ItemCloning.Enabled || (items.Length <= 0))
      {
        return;
      }

      var item = items[0];
      var itemIDs = new ListString('|');
      foreach (Item it in items)
      {
        itemIDs.Add(it.ID.ToString());
      }

      var parameters = new NameValueCollection();
      parameters.Add("database", item.Database.Name);
      parameters.Add("items", itemIDs.ToString());
      parameters.Add("language", item.Language.ToString());
      parameters.Add("mode", "smart");

      var clientPage = Context.ClientPage;
      Assert.IsNotNull(clientPage, "clientPage");

      clientPage.Start("uiCloneItems", new CopyItemsArgs { Parameters = parameters });
    }
  }
}