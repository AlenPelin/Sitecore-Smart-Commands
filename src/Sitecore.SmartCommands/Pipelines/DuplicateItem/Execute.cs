namespace Sitecore.SmartCommands.Pipelines.DuplicateItem
{
  using System;
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Diagnostics;
  using Sitecore.Globalization;
  using Sitecore.Web.UI.Sheer;

  public sealed class Execute
  {
    [UsedImplicitly]
    public void Process([NotNull] ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      var parameters = args.Parameters;
      Assert.IsNotNull(parameters, "parameters");

      if (!string.Equals(parameters["mode"], "smart", StringComparison.OrdinalIgnoreCase))
      {
        return;
      }

      var databaseName = parameters["database"];
      Assert.IsNotNull(databaseName, "databaseName");

      var database = Factory.GetDatabase(databaseName);
      Assert.IsNotNull(database, "database");

      var name = parameters["name"];
      Assert.IsNotNull(name, "name");

      var sourceId = parameters["id"];

      var languageName = parameters["language"];
      Language language;
      if (!Language.TryParse(languageName, out language))
      {
        language = Context.Language;
      }

      var item = database.GetItem(ID.Parse(sourceId), language);
      if (item == null)
      {
        SheerResponse.Alert("Item not found.", new string[0]);
        args.AbortPipeline();
        return;
      }

      var parent = item.Parent;
      if (parent == null)
      {
        SheerResponse.Alert("Cannot duplicate the root item.", new string[0]);
        args.AbortPipeline();
        return;
      }

      if (!parent.Access.CanCreate())
      {
        SheerResponse.Alert(Translate.Text("You do not have permission to duplicate \"{0}\".", new object[] { item.DisplayName }), new string[0]);
        args.AbortPipeline();
        return;
      }

      var workflow = Context.Workflow;
      Assert.IsNotNull(workflow, "workflow");

      Log.Audit(this, "Duplicate item: {0}", new string[] { AuditFormatter.FormatItem(item) });
      var copyItem = workflow.DuplicateItem(item, name);
      parameters["copyId"] = copyItem.ID.ToString();
    }
  }
}