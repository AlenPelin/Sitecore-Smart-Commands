namespace Sitecore.Support.Pipelines.DuplicateItem
{
  using System;
  using Sitecore.Diagnostics;
  using Sitecore.Web.UI.Sheer;

  public abstract class AbstractProcessor
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

      this.DoProcess(args);
    }

    protected abstract void DoProcess([NotNull] ClientPipelineArgs args);
  }
}