using System.Linq;
using NUnit.Framework;
using OmniSharp.Configuration;
using OmniSharp.FindUsages;
using OmniSharp.Parser;
using OmniSharp.Rename;
using OmniSharp.Solution;
using Should;

namespace OmniSharp.Tests.Rename
{
    [TestFixture]
    public class SimpleRenameTests
    {
        private string Rename(string buffer, string renameTo)
        {
            var location = TestHelpers.GetLineAndColumnFromDollar(buffer);
            buffer = buffer.Replace("$", "");
            
            var solution = new FakeSolutionBuilder()
                .AddFile(buffer)
                .Build();
            
            var bufferParser = new BufferParser(solution);
            var renameHandler = new RenameHandler(solution, bufferParser, new OmniSharpConfiguration(), new FindUsagesHandler(bufferParser, solution, new ProjectFinder(solution)));
            var request = new RenameRequest
                {
                    Buffer = buffer,
                    Column = location.Column - 1,
                    Line = location.Line,
                    RenameTo = renameTo,
                    FileName = "myfile"
                };

            var response = renameHandler.Rename(request);
            if(response.Changes.Any())
                return response.Changes.First().Buffer;
            return buffer;
        }

        [Test]
        public void Should_rename_variable()
        {
            Rename(
@"public class MyClass
{
    public MyClass()
    {
        var s$ = String.Empty;
    }
}", "str").ShouldEqual(
@"public class MyClass
{
    public MyClass()
    {
        var str = String.Empty;
    }
}"
 );
        }

        [Test]
        public void Should_rename_variable_and_usage()
        {
            Rename(
@"public class MyClass
{
    public MyClass()
    {
        var s$ = ""s"";
        s = s + s;
    }
}", "str")
  .ShouldEqual(
@"public class MyClass
{
    public MyClass()
    {
        var str = ""s"";
        str = str + str;
    }
}"
 );
        }

        [Test]
        public void Should_rename_field_and_usage()
        {
            Rename(
@"public class MyClass
{
    private string _s;
    public MyClass()
    {
        _s$ = ""s"";
    }
}", "_str")
  .ShouldEqual(
@"public class MyClass
{
    private string _str;
    public MyClass()
    {
        _str = ""s"";
    }
}"
 );
        }

        [Test]
        public void Should_rename_method()
        {
            Rename(
@"public class MyClass
{
    public MyClass()
    {
        MyMethod$();
    }

    void MyMethod()
    {
        
    }
}", "RenamedMethod")
.ShouldEqual(
@"public class MyClass
{
    public MyClass()
    {
        RenamedMethod();
    }

    void RenamedMethod()
    {
        
    }
}"
);            
        }

        [Test]
        public void Should_not_bomb_when_cursor_is_not_on_renameable()
        {
            Rename("pub$", "").ShouldEqual("pub");
        }

        [Test]
        public void Should_rename_type_and_constructors()
        {
            Rename(
@"public class MyCla$ss
{
    public MyClass()
    {
    }

    public MyClass(int param)
    {
    }
}", "Renamed")
.ShouldEqual(
@"public class Renamed
{
    public Renamed()
    {
    }

    public Renamed(int param)
    {
    }
}"
);          
        }

        [Test]
        public void Should_rename_type_and_constructors_from_constructor_rename()
        {
            Rename(
@"public class MyClass
{
    public My$Class()
    {
    }

    public MyClass(int param)
    {
    }
}", "Renamed")
.ShouldEqual(
@"public class Renamed
{
    public Renamed()
    {
    }

    public Renamed(int param)
    {
    }
}"
);
        }

        [Test]
        public void Should_rename_derived_type_usages()
        {
            Rename(
@"public class Request
{
    public string Col$umn { get; set; }
}

public class FindUsagesRequest : Request {}

public class Handler
{
    public Handler()
    {
        var req = new FindUsagesRequest();
        req.Column = 1;
    }
}", "Col"

 ).ShouldEqual(
@"public class Request
{
    public string Col { get; set; }
}

public class FindUsagesRequest : Request {}

public class Handler
{
    public Handler()
    {
        var req = new FindUsagesRequest();
        req.Col = 1;
    }
}");
        }

        [Test]
        public void Should_rename_variable_multiple_times_within_line()
        {
            Rename(@"
            public class MultipleTimes
            {
                public void SomeMethod()
                {
                    float test;
                    test = te$st * 10;
                }
            }", "testing")
            .ShouldEqual(@"
            public class MultipleTimes
            {
                public void SomeMethod()
                {
                    float testing;
                    testing = testing * 10;
                }
            }");
        }

        [Test]
        public void Should_rename_generic_field()
        {
            Rename(@"
            using System.Collections.Generic;

            class SampleClass
            {
                public List<string> Test$Field = new List<string>();

                public SampleClass()
                {
                    TestField.Add(""value1"");
                }
            }", "Renamed")
              .ShouldEqual(@"
            using System.Collections.Generic;

            class SampleClass
            {
                public List<string> Renamed = new List<string>();

                public SampleClass()
                {
                    Renamed.Add(""value1"");
                }
            }");
        }

        [Test]
        public void Should_not_rename_wrong_overloads()
        {
            Rename(@"
            class OverloadTest
            {
                public void Overload$edFunction(int a)
                {
                    OverloadedFunction(""test"");
                }
                public void OverloadedFunction(string str)
                {
                    OverloadedFunction(1);
                }
            }", "IntOverload")
            .ShouldEqual(@"
            class OverloadTest
            {
                public void IntOverload(int a)
                {
                    OverloadedFunction(""test"");
                }
                public void OverloadedFunction(string str)
                {
                    IntOverload(1);
                }
            }");
        }
    }
}
