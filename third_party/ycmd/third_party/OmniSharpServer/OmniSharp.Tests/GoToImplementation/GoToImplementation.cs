using System.Linq;
using NUnit.Framework;
using OmniSharp.Common;
using OmniSharp.GotoImplementation;
using OmniSharp.Parser;
using OmniSharp.Solution;
using OmniSharp.Tests.Rename;

namespace OmniSharp.Tests.GotoImplementation
{
    [TestFixture]
    public class GotoImplementationTests
    {
        [Test]
        public void Should_find_type_implementations_in_same_file()
        {
            const string editorText =
                @"
public class BaseClass {}
public class DerivedClassA : BaseClass {}
public class DerivedClassB : BaseClass {}
public class DerivedClassC : BaseClass {}
";
            var fileName = "test.cs";
            
            var solution = new FakeSolutionBuilder()
                .AddFile(editorText, fileName)
                .Build();
            
            var gotoImplementationResponse = FindDerivedMembersAsQuickFixes(solution, new GotoImplementationRequest
                {
                    Buffer = editorText,
                    Line = 2,
                    Column = 14, // At word 'BaseClass'
                    FileName = fileName
                });

            var quickFixes = gotoImplementationResponse.QuickFixes.ToArray();
            Assert.AreEqual(3, quickFixes.Length);
            quickFixes[0].Text.Trim().ShouldEqual("public class DerivedClassA : BaseClass {}");
            quickFixes[1].Text.Trim().ShouldEqual("public class DerivedClassB : BaseClass {}");
            quickFixes[2].Text.Trim().ShouldEqual("public class DerivedClassC : BaseClass {}");
        }

        [Test]
        public void Should_find_type_implementations_in_all_files()
        {
            const string editorText1 =
                @"
public abstract class BaseClass {}
public class DerivedClassA : BaseClass {}
";
            var fileName1 = "base.cs";

            const string editorText2 =
                @"
public class DerivedClassB : BaseClass {}
";
            var fileName2 = "derived.cs";

            var solution = new FakeSolutionBuilder()
                           .AddFile(editorText1, fileName1)
                           .AddFile(editorText2, fileName2)
                           .Build();

            var gotoImplementationResponse = FindDerivedMembersAsQuickFixes(solution, new GotoImplementationRequest
                {
                    Buffer = editorText1,
                    Line = 2,
                    Column = 23, // At word 'BaseClass'
                    FileName = fileName1
                });

            var quickFixes = gotoImplementationResponse.QuickFixes.ToArray();
            Assert.AreEqual(2, quickFixes.Length);
            quickFixes[0].Text.Trim().ShouldEqual("public class DerivedClassA : BaseClass {}");
            quickFixes[1].Text.Trim().ShouldEqual("public class DerivedClassB : BaseClass {}");
        }
      
        [Test]
        public void Should_find_type_implementations_in_all_projects()
        {
            const string editorText1 =
                @"
public interface IBase {}
public class DerivedClassA : IBase {}
";
            var fileName1 = "base.cs";

            const string editorText2 =
                @"
public class DerivedClassB : IBase {}
";
            var fileName2 = "derived.cs";

            var solution = new FakeSolutionBuilder()
                           .AddFile(editorText1, fileName1)
                           .AddProject()
                           .AddFile(editorText2, fileName2)
                           .Build();

            var gotoImplementationResponse = FindDerivedMembersAsQuickFixes(solution, new GotoImplementationRequest
                {
                    Buffer = editorText1,
                    Line = 2,
                    Column = 18, // At word 'BaseClass'
                    FileName = fileName1
                });

            var quickFixes = gotoImplementationResponse.QuickFixes.ToArray();
            Assert.AreEqual(2, quickFixes.Length);
            quickFixes[0].Text.Trim().ShouldEqual("public class DerivedClassA : IBase {}");
            quickFixes[1].Text.Trim().ShouldEqual("public class DerivedClassB : IBase {}");
        }
        
        [Test]
        public void Should_find_member_implementations_in_same_file()
        {
            const string editorText =
                @"
public class BaseClass { public virtual void Test() {} }
public class DerivedClassA : BaseClass 
{ 
public override void Test() {} //DerivedClassA impl
}
public class DerivedClassB : BaseClass 
{ 
public override void Test() {} //DerivedClassB impl
}
";
            var fileName = "test.cs";

            var solution = new FakeSolutionBuilder()
                .AddFile(editorText, fileName)
                .Build();

            var gotoImplementationResponse = FindDerivedMembersAsQuickFixes(solution, new GotoImplementationRequest
                {
                    Buffer = editorText,
                    Line = 2,
                    Column = 46, //BaseClass Test
                    FileName = fileName
                });

            var quickFixes = gotoImplementationResponse.QuickFixes.ToArray();
            Assert.AreEqual(2, quickFixes.Length);
            quickFixes[0].Text.Trim().ShouldEqual("public override void Test() {} //DerivedClassA impl");
            quickFixes[1].Text.Trim().ShouldEqual("public override void Test() {} //DerivedClassB impl");
        }

        [Test]
        public void Should_find_member_implementations_in_all_files()
        {
            const string editorText1 =
                @"
public abstract class BaseClass 
{
public abstract void Test();
}
public class DerivedClassA : BaseClass 
{ 
public override void Test() {} //DerivedClassA impl
}
";
            var fileName1 = "base.cs";

            const string editorText2 =
                @"
public class DerivedClassB : BaseClass 
{ 
public override void Test() {} //DerivedClassB impl
}
";
            var fileName2 = "derived.cs";

            var solution = new FakeSolutionBuilder()
                           .AddFile(editorText1, fileName1)
                           .AddFile(editorText2, fileName2)
                           .Build();

            var gotoImplementationResponse = FindDerivedMembersAsQuickFixes(solution, new GotoImplementationRequest
                {
                    Buffer = editorText1,
                    Line = 4,
                    Column = 22, //BaseClass Test
                    FileName = fileName1
                });

            var quickFixes = gotoImplementationResponse.QuickFixes.ToArray();
            Assert.AreEqual(2, quickFixes.Length);
            quickFixes[0].Text.Trim().ShouldEqual("public override void Test() {} //DerivedClassA impl");
            quickFixes[1].Text.Trim().ShouldEqual("public override void Test() {} //DerivedClassB impl");
        }

        [Test]
        public void Should_find_member_implementations_in_all_projects()
        {
            const string editorText1 =
                @"
public interface IBase 
{
void Test();
}
public class ImplementationA : IBase
{ 
public void Test() {} //ImplementationA
}
";
            var fileName1 = "base.cs";

            const string editorText2 =
                @"
public class ImplementationB : IBase 
{ 
public void Test() {} //ImplementationB
}
";
            var fileName2 = "derived.cs";

            var solution = new FakeSolutionBuilder()
                           .AddFile(editorText1, fileName1)
                           .AddProject()
                           .AddFile(editorText2, fileName2)
                           .Build();

            var gotoImplementationResponse = FindDerivedMembersAsQuickFixes(solution, new GotoImplementationRequest
                {
                    Buffer = editorText1,
                    Line = 4,
                    Column = 6, // IBase Test
                    FileName = fileName1
                });

            var quickFixes = gotoImplementationResponse.QuickFixes.ToArray();
            Assert.AreEqual(2, quickFixes.Length);
            quickFixes[0].Text.Trim().ShouldEqual("public void Test() {} //ImplementationA");
            quickFixes[1].Text.Trim().ShouldEqual("public void Test() {} //ImplementationB");
        }


        static QuickFixResponse FindDerivedMembersAsQuickFixes(FakeSolution solution, GotoImplementationRequest request)
        {
            var handler = new GotoImplementationHandler(solution, new BufferParser(solution), new ProjectFinder(solution));
            var gotoImplementationResponse = handler.FindDerivedMembersAsQuickFixes(request);
            return gotoImplementationResponse;
        }
    }
}
