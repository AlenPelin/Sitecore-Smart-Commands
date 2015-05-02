namespace Sitecore.Essentials.Shell.Framework.Commands
{
  using System;
  using System.Collections.Specialized;
  using System.Web;
  using Sitecore.Data;
  using Sitecore.Diagnostics;
  using Sitecore.Shell.Framework.Commands;
  using Sitecore.Web;
  using Sitecore.Web.UI.HtmlControls;
  using Sitecore.Web.UI.Sheer;

  [UsedImplicitly]
  public class ResetField : Command
  {
    public override void Execute([NotNull] CommandContext context)
    {
      Assert.ArgumentNotNull(context, "context");
      
      var pageContext = Context.Page;
      Assert.IsNotNull(pageContext, "pageContext");

      var page = pageContext.Page;
      Assert.IsNotNull(page, "page");

      var parameters = context.Parameters;
      Assert.IsNotNull(parameters, "parameters");

      var controlId = parameters["id"];
      Assert.IsNotNull(controlId, "controlId");

      var control = page.FindControl(controlId) as Control;
      Assert.IsNotNull(control, "control");

      var attr = control.ControlAttributes;
      Assert.IsNotNull(attr, "attr");

      var uriStarts = attr.IndexOf("sitecore://", StringComparison.Ordinal);
      Assert.IsTrue(uriStarts >= 0, "uriStarts");

      var uriString = attr.Substring(uriStarts);
      Assert.IsNotNull(uriString, "uriString");

      var uriEnds = uriString.IndexOf("\"", StringComparison.Ordinal);
      Assert.IsTrue(uriEnds >= 0, "uriEnds");

      var uri = HttpUtility.HtmlDecode(uriString.Substring(0, uriEnds));
      Assert.IsNotNull(uri, "uri");

      var itemId = new ItemUri(uri).ItemID;
      Assert.IsNotNull(itemId, "itemId");
      
      var queryString = WebUtil.ParseUrlParameters(uri);
      Assert.IsNotNull(queryString, "queryString");

      var fieldId = queryString["fld"];
      Assert.IsNotNull(fieldId, "fieldId");

      var contentDatabase = Sitecore.Context.ContentDatabase;
      Assert.IsNotNull(contentDatabase, "contentDatabase");

      var item = contentDatabase.GetItem(itemId);
      Assert.IsNotNull(item, "item");

      var field = item.Fields[fieldId];
      if (field == null || field.InheritsValueFromOtherItem)
      {
        // nothing to reset
        return;
      }

      var pipelineParameters = new NameValueCollection();
      pipelineParameters["itemId"] = itemId.ToString();
      pipelineParameters["fieldId"] = fieldId;

      var clientPage = Context.ClientPage;
      Assert.IsNotNull(clientPage, "clientPage");

      clientPage.Start(this, "Run", pipelineParameters);
    }

    [UsedImplicitly]
    public void Run([NotNull] ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      
      var parameters = args.Parameters;
      Assert.IsNotNull(parameters, "parameters");

      if (!SheerResponse.CheckModified())
      {
        return;
      }

      if (!args.IsPostBack)
      {
        var clientPage = Context.ClientPage;
        Assert.IsNotNull(clientPage, "clientPage");

        SheerResponse.Confirm("Are you sure that you want to reset this field?");
        
        args.WaitForPostBack();
        return;
      }

      if (args.Result == "no")
      {
        return;
      }

      SheerResponse.Eval("scForm.postEvent(this,event,'item:refresh');");
    }
  }
}