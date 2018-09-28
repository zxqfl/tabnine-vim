using NUnit.Framework;
using Should;

namespace OmniSharp.Tests.UnitTesting
{
    [TestFixture]
    public class TestContextInformationTests
    {
        [Test]
        public void Should_get_method_name_with_cursor_on_method_name()
        {
            @"
public class TestClass
{
    [Test]
    public void ThisIs$ATest()
    {
    }
}".GetContextInformation().MethodName.ShouldEqual("ThisIsATest");
        }

        [Test]
        public void Should_get_method_name_with_cursor_on_method_body()
        {
            @"
public class TestClass
{
    [Test]
    public void ThisIsATest()
    {
        int ten = 1$0;
    }
}".GetContextInformation().MethodName.ShouldEqual("ThisIsATest");
        }

        [Test]
        public void Should_get_method_name_with_cursor_on_method_access_modifier()
        {
            @"
public class TestClass
{
    [Test]
    publ$ic void ThisIsATest()
    {
        int ten = 10;
    }
}".GetContextInformation().MethodName.ShouldEqual("ThisIsATest");
        }

        [Test]
        public void Should_get_method_name_with_cursor_on_test_attribute()
        {
            @"
public class TestClass
{
    [$Test]
    public void ThisIsATest()
    {
    }
}".GetContextInformation().MethodName.ShouldEqual("ThisIsATest");
        }

        [Test]
        public void Should_not_get_method_name_with_cursor_between_methods()
        {
            @"
public class TestClass
{
    [Test]
    public void ThisIsATest()
    {
    }
    $
    [Test]
    public void ThisIsAnotherTest()
    {
    }
}".GetContextInformation().MethodName.ShouldBeNull();
        }

        [Test]
        public void Should_get_type_name()
        {
            @"
public class TestClass
{
    [$Test]
    public void ThisIsATest()
    {
    }
}".GetContextInformation().TypeName.ShouldEqual("TestClass");
        }

        [Test]
        public void Should_get_fully_qualified_type_name()
        {
            @"
namespace Namespace.Something
{
    public class TestClass
    {
        [$Test]
        public void ThisIsATest()
        {
        }
    }
}".GetContextInformation().TypeName.ShouldEqual("Namespace.Something.TestClass");
        }

        [Test]
        public void Should_get_fully_qualified_type_name_with_cursor_above_class_definition()
        {
            @"
namespace Namespace.Something
{$
    public class TestClass
    {
        [Test]
        public void ThisIsATest()
        {
        }
    }
}".GetContextInformation().TypeName.ShouldEqual("Namespace.Something.TestClass");
        }

        [Test]
        public void Should_get_fully_qualified_type_name_with_cursor_above_namespace()
        {
            @"$
namespace Namespace.Something
{
    public class TestClass
    {
        [Test]
        public void ThisIsATest()
        {
        }
    }
}".GetContextInformation().TypeName.ShouldEqual("Namespace.Something.TestClass");
        }

    }
}
