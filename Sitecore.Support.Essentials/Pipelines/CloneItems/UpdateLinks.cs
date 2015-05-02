namespace Sitecore.Support.Pipelines.CloneItems
{
  using System;
  using Sitecore.Diagnostics;
  using Sitecore.Shell.Framework.Pipelines;
  using Sitecore.Support.Jobs;

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

      foreach (var copy in copies)
      {
        if (this.isAsync)
        {
          ReferenceReplacementJob.StartAsync(copy.Source, copy);
        }
        else
        {
          ReferenceReplacementJob.Start(copy.Source, copy);
        }
      }

      // if mode is smart then this should be the last processor
      args.AbortPipeline();
    }
  }
}
