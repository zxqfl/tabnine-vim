using NUnit.Framework;
using Should;
using System.Linq;
using OmniSharp.AutoComplete;

namespace OmniSharp.Tests.AutoComplete
{
    [TestFixture]
    public class ConstructorParameterCompletions : CompletionTestBase
    {
        [Test]
        public void Should_return_all_constructors()
        {
            DisplayTextFor(
                @"public class MyClass {
                            public MyClass() {}
                            public MyClass(int param) {}
                            public MyClass(string param) {}
                        }

                        public class Class2 {
                            public Class2()
                            {
                                var c = new My$
                            }
                        }")
                .ShouldContainOnly(
                    "MyClass()",
                    "MyClass(int param)",
                    "MyClass(string param)");
        }

        [Test]
        public void Should_return_all_constructors_using_camel_case_completions()
        {
            DisplayTextFor(
                @"  public class MyClassA {
                        public MyClassA() {}
                        public MyClassA(int param) {}
                        public MyClassA(string param) {}
                    }

                    public class Class2 {
                        public Class2()
                        {
                            var c = new mca$
                        }
                    }")
                .ShouldContainOnly(
                    "MyClassA()",
                    "MyClassA(int param)",
                    "MyClassA(string param)");
        }

        [Test]
        public void Should_return_no_completions()
        {
            DisplayTextFor(
                @"  public class MyClassA {
                        public MyClassA() {}
                        public MyClassA(int param) {}
                        public MyClassA(string param) {}
                    }

                    public class Class2 {
                        public Class2()
                        {
                            var c = new zzz$
                        }
                    }").ShouldBeEmpty();
        }

        [Test]
        public void Should_not_return_ctor_for_system_diagnostics()
        {
            DisplayTextFor(
                @"  public class MyClass {
                            public MyClass() {
                                System.Diagnostics.$
                            }
                        }").ShouldNotContain(".ctor");
        }

        [Test]
        public void Should_return_debugger_when_including_importable_type()
        {
            DisplayTextFor(
                @"  public class MyClass {
                            public MyClass() {
                                new Debug$
                            }
                        }", true).ShouldContain("Debugger() [Using System.Diagnostics]");
        }

        [Test]
        public void Should_return_namespace_import_for_system_diagnostics()
        {
            CompletionsDataFor(
                @"  public class MyClass {
                            public MyClass() {
                                new Debug$
                            }
                        }", true).OfType<CompletionData>().Select(i => i.RequiredNamespaceImport).ShouldContain("System.Diagnostics");
        }

        [Test]
        public void Should_sort_imported_types_after_unimported_types()
        {
            var completions = DisplayTextFor(
                @"  public class ObjZoo { }
                    public class MyClass {
                            public MyClass() {
                                new Obj$
                            }
                        }", true)
                        .ToList();
            var zooIndex = completions.IndexOf("ObjZoo()");
            var objectIndex = completions.IndexOf("Object() [Using System]");
            Assert.Greater(zooIndex, -1);
            Assert.Greater(objectIndex, -1);
            Assert.Greater(objectIndex, zooIndex, "ObjZoo should be ordered before Object");
        }

        [Test]
        public void Should_mark_all_imported_types_overwrites()
        {
            var completions = CompletionsDataFor(
                @"  public class MyClass {
                            public MyClass() {
                                new StreamReader$
                            }
                        }", true)
            .Where(i => i.CompletionText.StartsWith("StreamReader("))
            .Select(i => i.DisplayText)
            ;
            Assert.That(completions, Is.Not.Empty);
            Assert.That(completions, Is.All.ContainsSubstring("[Using System.IO]"));
        }

        [Test]
        public void Should_not_close_parenthesis_for_constructor_with_parameter()
        {
            CompletionsFor(
                @"public class MyClass {
                    public MyClass(int param) {}
                }

                public class Class2 {
                    public Class2()
                    {
                        var c = new My$
                    }
                }")
                .ShouldContainOnly("MyClass(");
        }

        [Test]
        public void Should_close_parentheses_for_constructor_without_parameter()
        {
            CompletionsFor(
                @"public class MyClass {
                    public MyClass() {}
                }

                public class Class2 {
                    public Class2()
                    {
                        var c = new My$
                    }
                }")
                .ShouldContainOnly("MyClass()");
        }
    }
}
