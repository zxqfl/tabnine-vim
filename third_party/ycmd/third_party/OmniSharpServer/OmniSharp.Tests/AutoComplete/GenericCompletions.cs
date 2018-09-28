using NUnit.Framework;
using Should;

namespace OmniSharp.Tests.AutoComplete
{
    [TestFixture]
    public class GenericCompletions : CompletionTestBase
    {
        [Test]
        public void Should_complete_extension_method()
        {
            DisplayTextFor(
                @"using System.Collections.Generic;
            using System.Linq;

            public class A {
                public A()
                {
                    string s;
                    s.MyEx$
                }
            }

            public static class StringExtensions
            {
                public static string MyExtension(this string s)
                {
                    return s;
                }

                public static string MyExtension(this string s, int i)
                {
                    return s;
                }
            }
            ").ShouldContainOnly("string MyExtension()", "string MyExtension(int i)");
        }

        [Test]
        public void Should_complete_generic_completion()
        {
            DisplayTextFor(
                @"using System.Collections.Generic;
            public class Class1 {
                public Class1()
                {
                    var l = new List<string>();
                    l.ad$
                }
            }")
                .ShouldContain(
                    "void Add(string item)",
                    "void AddRange(IEnumerable<string> collection)"); 
        }

        [Test]
        public void Should_add_angle_bracket_to_generic_completion()
        {
            CompletionsFor(
                @"using System.Collections.Generic;
            public class Class1 {
                public Class1()
                {
                    var l = new Lis$
                }
            }")
                .ShouldContain("List<");
        }
        
        [Test]
        public void Should_add_angle_bracket_to_generic_completion_initialiser()
        {
            CompletionsFor(
                @"using System.Collections.Generic;
            public class Class1 {
                public Class1()
                {
                    Lis$
                }
            }")
                .ShouldContain("List<");
        }

        [Test]
        public void Should_not_add_angle_bracket_to_method_as_generic_type_is_known()
        {
            CompletionsFor(
                @"using System.Collections.Generic;
            public class Class1 {
                public Class1()
                {
                    var l = new List<int>();
                    l.Ad$
                }
            }")
                .ShouldContain("Add(");
        }

        [Test]
        public void Should_not_add_bracket_to_method_as_generic_type_is_known()
        {
            CompletionsFor(
                @"using System.Collections.Generic;
            public class Class1 {
                public Class1()
                {
                    var l = new List<int>();
                    l.Conv$
                }
            }")
                .ShouldContain("ConvertAll(");
        }

        [Test]
        public void Should_not_add_angle_bracket_when_TSource_is_same_as_callee()
        {
            CompletionsFor(
                @"using System.Collections.Generic;
            using System.Linq;
            public class Class1 {
                public Class1()
                {
                    var l = new List<int>();
                    l.Distin$
                }
            }")
                .ShouldContain("Distinct(");
        }

        [Test]
        public void Should_not_add_angle_bracket_when_type_can_be_inferred_from_parameter()
        {
            CompletionsFor(
                    @"public class test
                    {
                        public void GenericMethod<T>(T whatever)
                        {
                        }

                        public void Caller()
                        {
                        
                            GenericMe$
                    ").ShouldContain("GenericMethod(");
        }

        [Test]
        public void Should_add_angle_bracket_when_type_cannot_be_inferred_from_parameter()
        {
            CompletionsFor(
                    @"public class test
                    {
                        public void GenericMethod<T2>(T whatever)
                        {
                        }

                        public void Caller()
                        {
                        
                            GenericMe$
                    ").ShouldContain("GenericMethod<");
        }

        [Test]
        public void Should_not_add_angle_bracket_to_known_type_extension_method_with_enumerable_parameter()
        {
            CompletionsFor(
                    @"
                    using System.Collections.Generic;
                    public static class Extensions
                    {
                        public static void ShouldContain<T>(this IEnumerable<T> actual, IEnumerable<T> expected)
                        {
                        }
                    }

                    public class test
                    {
                        public void Caller()
                        {
                            new string[].ShouldC$
                    ").ShouldContain("ShouldContain(");
        }

        [Test]
        public void Should_not_add_angle_bracket_to_known_type_extension_method_with_no_parameters()
        {
            CompletionsFor(
                    @"
                    using System.Collections.Generic;
                    public static class Extensions
                    {
                        public static void ShouldNotBeNull<T>(this T expected)
                        {
                        }
                    }

                    public class test
                    {
                        public void Caller()
                        {
                            1.ShouldN$
                    ").ShouldContain("ShouldNotBeNull()");
            "1".ShouldNotBeNull();
        }
    }
}
