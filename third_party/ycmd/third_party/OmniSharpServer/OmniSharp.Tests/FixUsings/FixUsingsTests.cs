
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OmniSharp.CodeIssues;
using OmniSharp.Common;
using OmniSharp.Configuration;
using OmniSharp.Parser;
using Should;
using OmniSharp.Tests.Rename;
namespace OmniSharp.Tests.FixUsings
{
    [TestFixture]
    public class FixUsingsTests
    {
        [Test]
        public void Should_remove_unused_using()
        {
            @"
using System;
public class {}"
            .FixUsings().ShouldBeEmpty();
        }

        [Test]
        public void Should_remove_multiple_unused_usings()
        {
            @"
using System;
using System;
using System;
using System;
using System;
using System;
using System;
using System;
using System;
public class {}"
            .FixUsings().ShouldBeEmpty();
        }

        [Test]
        public void Should_sort_usings()
        {
            @"
using ns2;
using ns1;

public class test {
    class1 ns1 = new class1();
    class2 ns2 = new class2();
}

namespace ns1
{
    public class class1{}
}

namespace ns2
{
    public class class2{}
}
"
.FixUsings()
.ShouldEqual("using ns1;", "using ns2;");
        }

        [Test]
        public void Should_add_using()
        {
            @"
public class test {
    class1 ns1 = new class1();
}

namespace ns1
{
    public class class1{}
}"
.FixUsings()
.ShouldEqual("using ns1;");
        }

        [Test]
        public void Should_add_two_usings()
        {
            @"
public class test {
    class1 ns1 = new class1();
    class2 ns2 = new class2();
}

namespace ns1
{
    public class class1{}
}

namespace ns2
{
    public class class2{}
}
".FixUsings()
.ShouldEqual("using ns1;", "using ns2;");
        }

        [Test]
        public void Should_add_using_for_extension_method()
        {
            @"
public class test {
    public test()
    {
        ""string"".Whatever();
    }
}

namespace ns1
{
    public static class StringExtension
    {
        public static void Whatever(this string astring) {}
    }
}
".FixUsings()
.ShouldEqual("using ns1;");
        }

        [Test]
        public void Should_add_using_for_method()
        {
            @"
public class test {
    public test()
    {
        Console.WriteLine(""test"");
    }
}
".FixUsings()
.ShouldEqual("using System;");
        }

        [Test]
        public void Should_add_using_for_new_class()
        {
            @"
public class test {
    public test()
    {
        var s = new String('x');
    }
}
".FixUsings()
.ShouldEqual("using System;");
        }

        [Test]
        public void Should_add_linq()
        {
            @"
public class test {
    public test()
    {
        var first = new List<string>().First();
    }
}
".FixUsings()
.ShouldEqual("using System.Collections.Generic;", "using System.Linq;");
        }

        [Test]
        public void Should_also_add_linq()
        {
            @"
public class test {
    public test()
    {
        var first = new List<string>().First(c => c == 'x');
    }
}
".FixUsings()
.ShouldEqual("using System.Collections.Generic;", "using System.Linq;");
        }

        [Test]
        public void Should_add_linq_for_query()
        {

            @"
using System.Collections.Generic;
public class test {
    public test()
    {
        var list = new List<string>();
        var whatever = (from s in list select s);
    }
}
".FixUsings()
.ShouldEqual("using System.Collections.Generic;", "using System.Linq;");
        }

        [Test]
        public void Should_not_add_linq_twice()
        {
            @"
using System.Collections.Generic;
using System.Linq;

public class test {
    public test()
    {
        var list = new List<string>();
        var whatever = (from s in list select s);
    }
}
".FixUsings()
 .ShouldEqual("using System.Collections.Generic;", "using System.Linq;");
        }

        [Test]
        public void Should_not_add_ambiguous_usings()
        {
            @"
public class test {
    class1 ns1 = new class1();
}

namespace ns1
{
    public class class1{}
}

namespace ns2
{
    public class class1{}
}
".FixUsings()
.ShouldBeEmpty();

        }

        [Test]
        public void Should_not_abort_when_ambiguous_usings_found()
        {
            @"
public class test {
    var l = new List<class1>()
    var ns1 = new class1();
    var s = new String('x');
}

namespace ns1
{
    public class class1{}
}

namespace ns2
{
    public class class1{}
}
".FixUsings()
                .ShouldEqual("using System;", "using System.Collections.Generic;");
        }

        [Test]
        public void Should_set_ambiguous_results()
        {
            @"
public class test {

    class1 ns1 = new class1();
    var s = new String('x');
}

namespace ns1
{
    public class class1{}
}

namespace ns2
{
    public class class1{}
}
".GetFixUsingsResponse().AmbiguousResults.First().Text
.ShouldEqual("`class1` is ambiguous");
        }
    }

    public static class FixUsingsExtension
    {
        public static FixUsingsResponse GetFixUsingsResponse(this string buffer)
        {
            var solution = new FakeSolutionBuilder().AddFile(buffer).Build();
            var bufferParser = new BufferParser(solution);
            var handler = new FixUsingsHandler(bufferParser, new Logger(Verbosity.Quiet), new OmniSharpConfiguration());
            var request = new Request();
            request.Buffer = buffer;
            request.FileName = "myfile";
            // line number should be irrelevant
            request.Line = int.MaxValue;
            var response = handler.FixUsings(request);
            return response;
        }

        public static IEnumerable<string> FixUsings(this string buffer)
        {
            var response = GetFixUsingsResponse(buffer);
            
            return response.Buffer.Split(new [] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Where(line => line.Contains("using"));
        }

    }
}
