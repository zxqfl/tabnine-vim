using NUnit.Framework;

namespace OmniSharp.Tests.AutoComplete
{
    [TestFixture]
    public class SnippetTests : CompletionTestBase
    {
        [Test]
        public void Should_template_generic_type_argument()
        {
            SnippetFor(
                @"using System.Collections.Generic;
            public class Class1 {
                public Class1()
                {
                    var l = new Lis$
                }
            }")
                .ShouldContain("List<${1:T}>()$0");        
        }

        [Test]
        public void Should_not_include_tsource_argument_type()
        {
            SnippetFor(
                @"using System.Collections.Generic;
            using System.Linq;
            public class Class1 {
                public Class1()
                {
                    var l = new List<string>();
                    l.Firs$
                }
            }")
                .ShouldContain("First()$0");        
        }

        [Test]
        public void Should_not_include_tresult_argument_type()
        {
            SnippetFor(
                @"using System.Collections.Generic;
            using System.Linq;
            public class Class1 {
                public Class1()
                {
                    var dict = new Dictionary<string, object>();
                    dict.Sel$
                }
            }")
                .ShouldContain("Select(${1:Func<TSource, TResult> selector})$0");
        }

        [Test]
        public void Should_template_field()
        {
            SnippetFor(
                @"using System.Collections.Generic;

            public class Class1 {
                public int someField;
                public Class1()
                {
                    somef$
                }
            }")
                .ShouldContain("someField$0");        
        }

        [Test]
        public void Should_return_all_constructors()
        {
            SnippetFor(
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
                    "MyClass()$0",
                    "MyClass(${1:int param})$0",
                    "MyClass(${1:string param})$0");
        }

        [Test]
        public void Should_return_all_constructors_with_trailing_code()
        {
            SnippetFor(
                @"public class MyClass {
                            public MyClass() {}
                            public MyClass(int param) {}
                            public MyClass(string param) {}
                        }

                        public class Class2 {
                            public Class2()
                            {
                                var c = new My$
                                if(2+2==4)
                                {
                                }
                            }
                        }")
                .ShouldContainOnly(
                    "MyClass()$0",
                    "MyClass(${1:int param})$0",
                    "MyClass(${1:string param})$0");
        }

        [Test]
        public void Should_template_generic_type_arguments()
        {
            SnippetFor(
                @"using System.Collections.Generic;
            public class Class1 {
                public Class1()
                {
                    var l = new Dict$
                }
            }")
                .ShouldContain("Dictionary<${1:TKey}, ${2:TValue}>()$0");      
        }

        [Test]
        public void Should_template_parameter()
        {
            SnippetFor(
                @"using System.Collections.Generic;
            public class Class1 {
                public Class1()
                {
                    var l = new Lis$
                }
            }")
                .ShouldContain("List<${1:T}>(${2:IEnumerable<T> collection})$0");      
        }

        [Test]
        public void Should_return_method_type_arguments_snippets()
        {
            SnippetFor(
                @"using System.Collections.Generic;
                public class Test {
                    public string Get<SomeType>()
                    {
                    }
                }
                public class Class1 {
                    public Class1()
                    {
                        var someObj = new Test();
                        someObj.G$
                    }
                }")
                .ShouldContain("Get<${1:SomeType}>()$0");
        }

    }
    
}