namespace Sitecore.Support.Pipelines.CopyItems
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Sitecore.Configuration;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Shell.Framework.Pipelines;
  using Sitecore.Support.Jobs;
  using Sitecore.Text;

  public sealed class UpdateLinks
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
    public void Process([NotNull] CopyItemsArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      var parameters = args.Parameters;
      Assert.IsNotNull(parameters, "parameters");

      if (!string.Equals(parameters["mode"], "smart", StringComparison.OrdinalIgnoreCase))
      {
        return;
      }
      
      var copies = args.Copies;
      Assert.IsNotNull(copies, "copies");

      var sources = GetItems(args).ToArray();
      for (var i = 0; i < copies.Length; i++)
      {
        var copy = copies[i];
        var source = sources[i];
        if (this.isAsync)
        {
          ReferenceReplacementJob.StartAsync(source, copy);
        }
        else
        {
          ReferenceReplacementJob.Start(source, copy);
        }
      }

      // if mode is smart then this should be the last processor
      args.AbortPipeline();
    }

    private static IEnumerable<Item> GetItems(CopyItemsArgs args)
    {
      var databaseName = args.Parameters["database"];
      var database = Factory.GetDatabase(databaseName);
      Assert.IsNotNull(database, "database");

      var listString = new ListString(args.Parameters["items"], '|');
      foreach (var idString in listString)
      {
        var item = database.GetItem(idString);
        if (item != null)
        {
          yield return item;
        }
      }
    }
  }
}
