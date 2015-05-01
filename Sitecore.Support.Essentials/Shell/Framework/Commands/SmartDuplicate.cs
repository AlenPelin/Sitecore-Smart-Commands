namespace Sitecore.Support.Shell.Framework.Commands
{
  using System.Collections.Specialized;
  using System.Linq;
  using Sitecore.Diagnostics;
  using Sitecore.Shell.Framework.Commands;

  [UsedImplicitly]
  public class SmartDuplicate : Duplicate
  {
    public override void Execute([NotNull] CommandContext context)
    {
      Assert.ArgumentNotNull(context, "context");

      var items = context.Items;
      Assert.IsNotNull(items, "items");

      var item = items.FirstOrDefault();
      if (item == null)
      {
        return;
      }

      var database = item.Database;
      Assert.IsNotNull(database, "database");

      var id = item.ID;
      Assert.IsNotNull(id, "id");

      var language = item.Language;
      Assert.IsNotNull(language, "language");

      var version = item.Version;
      Assert.IsNotNull(version, "version");

      var parameters = new NameValueCollection();
      parameters.Add("database", database.Name);
      parameters.Add("id", id.ToString());
      parameters.Add("language", language.ToString());
      parameters.Add("version", version.ToString());
      parameters.Add("mode", "smart");

      var clientPage = Context.ClientPage;
      Assert.IsNotNull(clientPage, "clientPage");

      clientPage.Start("uiDuplicateItem", parameters);
    }
  }
}