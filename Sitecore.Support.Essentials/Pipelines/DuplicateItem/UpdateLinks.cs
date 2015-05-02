namespace Sitecore.Support.Pipelines.DuplicateItem
{
  using System;
  using Sitecore.Configuration;
  using Sitecore.Diagnostics;
  using Sitecore.Support.Jobs;
  using Sitecore.Web.UI.Sheer;

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
      var database = Factory.GetDatabase(databaseName);
      Assert.IsNotNull(database, "database");

      var copyId = parameters["copyId"];
      Assert.IsNotNull(copyId, "copyId");

      var sourceId = parameters["id"];
      Assert.IsNotNull(sourceId, "sourceId");

      if (this.isAsync)
      {
        ReferenceReplacementJob.StartAsync(database, sourceId, copyId);
      }
      else
      {
        ReferenceReplacementJob.Start(database, sourceId, copyId);
      }

      // if mode is smart then this should be the last processor
      args.AbortPipeline();
    }
  }
}
