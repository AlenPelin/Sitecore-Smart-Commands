namespace Sitecore.Essentials.UnitTests
{
  using FluentAssertions;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using Sitecore.Essentials.Jobs;
  using Sitecore.FakeDb;

  [TestClass]
  public class ReferenceReplacementJobTests
  {
    [TestMethod]
    public void ReferenceReplacementJobTest()
    {
      using (var db = new Db("master"))
      {
        db.Configuration.Settings["LicenseFile"] = "C:\\Sitecore\\Keys\\license.xml";

        var sourceRoot = Sitecore.Data.ID.NewID;
        var sourceChild1 = Sitecore.Data.ID.NewID;
        var sourceGrandChild1 = Sitecore.Data.ID.NewID;
        var sourceChild2 = Sitecore.Data.ID.NewID;

        // TODO: make this test for versioned, unversioned and shared fields
        var sourceRootItem = 
          new DbItem("SourceRoot", sourceRoot)
          {
            new DbField("Root") { Value = sourceRoot.ToString(), Type = "droptree" },
            new DbField("Child1") { Value = sourceChild1.ToString(), Type = "droptree" },
            new DbField("GrandChild1") { Value = sourceGrandChild1.ToString(), Type = "droptree" },
            new DbField("Child2") { Value = sourceChild2.ToString(), Type = "droptree" },
          
            new DbItem("SourceChild1", sourceChild1)
            {
              new DbField("Root") { Value = sourceRoot.ToString(), Type = "droptree" },
              new DbField("Child1") { Value = sourceChild1.ToString(), Type = "droptree" },
              new DbField("GrandChild1") { Value = sourceGrandChild1.ToString(), Type = "droptree" },
              new DbField("Child2") { Value = sourceChild2.ToString(), Type = "droptree" },

              new DbItem("SourceGrandChild1", sourceGrandChild1) 
              {
                new DbField("Root") { Value = sourceRoot.ToString(), Type = "droptree" },
                new DbField("Child1") { Value = sourceChild1.ToString(), Type = "droptree" },
                new DbField("GrandChild1") { Value = sourceGrandChild1.ToString(), Type = "droptree" },
                new DbField("Child2") { Value = sourceChild2.ToString(), Type = "droptree" },
              }
            },
            new DbItem("SourceChild2", sourceChild2)
            {
              new DbField("Root") { Value = sourceRoot.ToString(), Type = "droptree" },
              new DbField("Child1") { Value = sourceChild1.ToString(), Type = "droptree" },
              new DbField("GrandChild1") { Value = sourceGrandChild1.ToString(), Type = "droptree" },
              new DbField("Child2") { Value = sourceChild2.ToString(), Type = "droptree" },
            }
          };

        db.Add(sourceRootItem);

        var item11 = db.GetItem(sourceRoot);
        var copyRoot = item11.Duplicate("copyRoot");

        ReferenceReplacementJob.Start(db.Database, sourceRoot, copyRoot.ID);
        var id = copyRoot.ID;
        copyRoot = db.Database.GetItem(id);
        copyRoot["Root"].Should().NotBeEmpty().And.NotBe(sourceRoot.ToString());
        copyRoot["Child1"].Should().NotBeEmpty().And.NotBe(sourceChild1.ToString());
        copyRoot["GrandChild1"].Should().NotBeEmpty().And.NotBe(sourceGrandChild1.ToString());
        copyRoot["Child2"].Should().NotBeEmpty().And.NotBe(sourceChild2.ToString());

        foreach (var descendant in copyRoot.Axes.GetDescendants())
        {
          descendant["Root"].Should().NotBeEmpty().And.NotBe(sourceRoot.ToString());
          descendant["Child1"].Should().NotBeEmpty().And.NotBe(sourceChild1.ToString());
          descendant["GrandChild1"].Should().NotBeEmpty().And.NotBe(sourceGrandChild1.ToString());
          descendant["Child2"].Should().NotBeEmpty().And.NotBe(sourceChild2.ToString());
        }
      }
    }
  }
}