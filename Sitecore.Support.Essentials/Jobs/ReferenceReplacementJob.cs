namespace Sitecore.Support.Jobs
{
  using System.Linq;
  using Sitecore.Data;
  using Sitecore.Data.Fields;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Globalization;
  using Sitecore.Jobs;
  using Sitecore.Links;
  using Sitecore.StringExtensions;

  public sealed class ReferenceReplacementJob
  {
    #region Fields and Constructors

    [NotNull]
    private readonly Database database;

    [NotNull]
    private readonly Item sourceRoot;

    private readonly int sourceRootLength;

    [NotNull]
    private readonly Item copyRoot;

    [NotNull]
    private readonly string copyRootPath;

    [UsedImplicitly]
    public ReferenceReplacementJob([NotNull] Item source, [NotNull] Item copy)
    {
      Assert.ArgumentNotNull(source, "source");
      Assert.ArgumentNotNull(copy, "copy");

      var length = source.Paths.FullPath.Length;
      var path = copy.Paths.FullPath;
      var sourceDatabase = source.Database;
      Assert.IsTrue(sourceDatabase == copy.Database, "items from different databases");

      // set read-only fields
      this.database = sourceDatabase;
      this.sourceRoot = source;
      this.sourceRootLength = length;
      this.copyRoot = copy;
      this.copyRootPath = path;
    }

    public ReferenceReplacementJob([NotNull] Database database, [NotNull] string sourceId, [NotNull] string copyId)
    {
      Assert.ArgumentNotNull(database, "database");
      Assert.ArgumentNotNull(sourceId, "sourceId");
      Assert.ArgumentNotNull(copyId, "copyId");

      var source = database.GetItem(sourceId);
      Assert.IsNotNull(source, "Source item {0} does not exist in {1} database".FormatWith(sourceId, database.Name));

      var copy = database.GetItem(copyId);
      Assert.IsNotNull(source, "Target item {0} does not exist in {1} database".FormatWith(sourceId, database.Name));

      var sourceLength = source.Paths.FullPath.Length;
      var copyPath = copy.Paths.FullPath;

      // set read-only fields
      this.database = source.Database;
      this.sourceRoot = source;
      this.sourceRootLength = sourceLength;
      this.copyRoot = copy;
      this.copyRootPath = copyPath;
    }

    public ReferenceReplacementJob([NotNull] Database database, [NotNull] ID sourceId, [NotNull] ID copyId)
    {
      Assert.ArgumentNotNull(database, "database");
      Assert.ArgumentNotNull(sourceId, "sourceId");
      Assert.ArgumentNotNull(copyId, "copyId");

      var source = database.GetItem(sourceId);
      Assert.IsNotNull(source, "Source item {0} does not exist in {1} database".FormatWith(sourceId, database.Name));

      var copy = database.GetItem(copyId);
      Assert.IsNotNull(source, "Target item {0} does not exist in {1} database".FormatWith(sourceId, database.Name));

      var sourceLength = source.Paths.FullPath.Length;
      var copyPath = copy.Paths.FullPath;
      
      // set read-only fields
      this.database = source.Database;
      this.sourceRoot = source;
      this.sourceRootLength = sourceLength;
      this.copyRoot = copy;
      this.copyRootPath = copyPath;
    }

    #endregion

    #region Static methods

    [UsedImplicitly]
    public static void Start([NotNull] Database database, [NotNull] string sourceId, [NotNull] string copyId)
    {
      Assert.ArgumentNotNull(database, "database");
      Assert.ArgumentNotNull(sourceId, "sourceId");
      Assert.ArgumentNotNull(copyId, "copyId");

      var job = new ReferenceReplacementJob(database, sourceId, copyId);
      job.Start();
    }

    [UsedImplicitly]
    public static void Start([NotNull] Database database, [NotNull] ID sourceId, [NotNull] ID copyId)
    {
      Assert.ArgumentNotNull(database, "database");
      Assert.ArgumentNotNull(sourceId, "sourceId");
      Assert.ArgumentNotNull(copyId, "copyId");

      var job = new ReferenceReplacementJob(database, sourceId, copyId);
      job.Start();
    }

    [UsedImplicitly]
    public static void Start([NotNull] Item source, [NotNull] Item copy)
    {
      Assert.ArgumentNotNull(source, "source");
      Assert.ArgumentNotNull(copy, "copy");

      var job = new ReferenceReplacementJob(source, copy);
      job.Start();
    }

    [UsedImplicitly]
    public static void StartAsync([NotNull] Database database, [NotNull] string sourceId, [NotNull] string copyId)
    {
      Assert.ArgumentNotNull(database, "database");
      Assert.ArgumentNotNull(sourceId, "sourceId");
      Assert.ArgumentNotNull(copyId, "copyId");

      var job = new ReferenceReplacementJob(database, sourceId, copyId);
      job.StartAsync();
    }

    [UsedImplicitly]
    public static void StartAsync([NotNull] Database database, [NotNull] ID sourceId, [NotNull] ID copyId)
    {
      Assert.ArgumentNotNull(database, "database");
      Assert.ArgumentNotNull(sourceId, "sourceId");
      Assert.ArgumentNotNull(copyId, "copyId");

      var job = new ReferenceReplacementJob(database, sourceId, copyId);
      job.StartAsync();
    }

    [UsedImplicitly]
    public static void StartAsync([NotNull] Item source, [NotNull] Item copy)
    {
      Assert.ArgumentNotNull(source, "source");
      Assert.ArgumentNotNull(copy, "copy");

      var job = new ReferenceReplacementJob(source, copy);
      job.StartAsync();
    }

    #endregion

    #region Public methods

    public void StartAsync()
    {
      var jobCategory = typeof(ReferenceReplacementJob).Name;
      var siteName = Context.Site == null ? "shell" : Context.Site.Name;
      var jobName = string.Format("ReferenceReplacement_{0}_{1}_{2}.", this.sourceRoot.Database.Name, this.sourceRoot.ID.ToShortID(), this.copyRoot.ID.ToShortID());
      var jobOptions = new JobOptions(jobName, jobCategory, siteName, this, "Start");

      JobManager.Start(jobOptions);
    }

    public void Start()
    {
      this.ProcessItem(this.sourceRoot, this.copyRoot);
    }

    #endregion

    #region Private methods

    private void ProcessItem([NotNull] Item source, [NotNull] Item copy)
    {
      Assert.ArgumentNotNull(source, "source");
      Assert.ArgumentNotNull(copy, "copy");

      var sourcePath = source.Paths.FullPath;
      var linksNeedProcessing = copy.Links.GetAllLinks(true, true).Where(x => this.NeedsProcessing(x));
      var languageGroups = linksNeedProcessing.GroupBy(x => x.SourceItemLanguage);
      foreach (var languageGroup in languageGroups)
      {
        var versionGroups = languageGroup.GroupBy(x => x.SourceItemVersion);
        foreach (var versionGroup in versionGroups)
        {
          var language = languageGroup.Key;
          var copyVersion = this.GetVersion(copy.ID, language, versionGroup) ?? copy;
          Assert.IsNotNull(copyVersion, string.Format("LinkDatabase is out of sync, cannot find item: {0}:{1}, language: {2}, version: {3}", database.Name, copy.ID, language.Name, versionGroup.Key.Number));

          copyVersion.Editing.BeginEdit();
          foreach (var link in versionGroup)
          {
            this.ProcessLink(copyVersion, link, sourcePath);
          }

          Log.Audit("Replace links: {0}".FormatWith(AuditFormatter.FormatItem(copyVersion)), this);
          copyVersion.Editing.EndEdit();
        }
      }

      var sourceChildren = source.Children;
      foreach (Item copyChild in copy.Children)
      {
        var sourceChild = sourceChildren.Single(x => x.Name.Equals(copyChild.Name) && x.Appearance.Sortorder == copyChild.Appearance.Sortorder);
        this.ProcessItem(sourceChild, copyChild);
      }
    }

    [CanBeNull]
    private Item GetVersion([NotNull] ID id, [NotNull] Language language, [NotNull] IGrouping<Data.Version, ItemLink> versionGroup)
    {
      Assert.ArgumentNotNull(id, "id");
      Assert.ArgumentNotNull(language, "language");
      Assert.ArgumentNotNull(versionGroup, "versionGroup");

      if (!string.IsNullOrEmpty(language.Name))
      {
        var version = versionGroup.Key;
        if (version.Number < 1)
        {
          return this.database.GetItem(id, language);
        }
        else
        {
          return this.database.GetItem(id, language, version);
        }
      }

      return null;
    }

    private bool NeedsProcessing([NotNull] ItemLink link)
    {
      Assert.ArgumentNotNull(link, "link");

      // prevent circular cloning chain
      if (link.SourceFieldID == FieldIDs.Source)
      {
        return false;
      }

      var linkTarget = link.GetTargetItem();
      if (linkTarget == null)
      {
        return false;
      }

      // if not descendant-or-self of this.sourceRoot
      if (linkTarget != this.sourceRoot && !linkTarget.Axes.IsDescendantOf(this.sourceRoot))
      {
        return false;
      }

      return true;
    }

    private void ProcessLink([NotNull] Item copyVersion, [NotNull] ItemLink link, [NotNull] string sourcePath)
    {
      Assert.ArgumentNotNull(copyVersion, "copyVersion");
      Assert.ArgumentNotNull(link, "link");
      Assert.ArgumentNotNull(sourcePath, "sourcePath");

      var linkTarget = link.GetTargetItem();
      Assert.IsNotNull(linkTarget, "linkTarget");

      var linkTargetPath = linkTarget.Paths.FullPath;

      // correct path = <copy> / <virtual path>
      var sourceVirtualPath = linkTargetPath.Substring(this.sourceRootLength);
      var correctLinkTargetPath = this.copyRootPath + sourceVirtualPath;
      var correctLinkTarget = this.database.GetItem(correctLinkTargetPath);
      if (correctLinkTarget == null)
      {
        Log.Warn("Cannot find corresponding item for {0} with path {1} in {2} database".FormatWith(sourcePath, correctLinkTargetPath, this.database.Name), this);
        return;
      }

      var field = copyVersion.Fields[link.SourceFieldID];
      var customField = FieldTypeManager.GetField(field);
      Assert.IsNotNull(customField, "customField");

      customField.Relink(link, correctLinkTarget);
    }

    #endregion
  }
}
