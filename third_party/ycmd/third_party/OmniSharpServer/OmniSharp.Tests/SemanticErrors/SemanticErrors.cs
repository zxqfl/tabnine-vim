using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using OmniSharp.SemanticErrors;
using OmniSharp.Common;
using Should;

namespace OmniSharp.Tests.SemanticErrors {
    [TestFixture]
    public class SemanticErrorsTests {
        [Test]
        public void Should_find_unknown_identifier() {
            const string editorText =
@"
public class TestClass {
    public void Test() {
        new IDontExist();
    }
}
";

            var errors = GetErrors(editorText);

            Assert.AreEqual(2, errors.Length);
            errors[0].Message.ShouldEqual("'IDontExist' is not a known identifier");
            errors[1].Message.ShouldEqual("'IDontExist' is not a known identifier");
        }

        [Test]
        public void Should_find_unknown_member() {
            const string editorText =
@"
public class TestClass {
    public void Test() {
        var test = ""Test"";
        test.Test();
    }
}
";

            var errors = GetErrors(editorText);

            Assert.AreEqual(2, errors.Length);
            errors[0].Message.ShouldEqual("'String' does not contain a definition for 'Test'");
            errors[1].Message.ShouldEqual("'String' does not contain a definition for 'Test'");
        }

        [Test]
        public void Should_find_invalid_conversion() {
            const string editorText =
@"
public class TestClass {
    public void Test() {
        bool test = ""Test"";
    }
}
";

            var errors = GetErrors(editorText);

            Assert.AreEqual(1, errors.Length);
            errors[0].Message.ShouldEqual("Cannot convert from System.String to System.Boolean");
        }

        [Test]
        public void Should_find_invalid_invocation() {
            const string editorText =
@"
public class TestClass {
    public void Test() {
        var test = ""Test"";
        test.GetHashCode(""TEST"");
    }
}
";

            var errors = GetErrors(editorText);

            Assert.AreEqual(1, errors.Length);
            errors[0].Message.ShouldEqual("Invocation error: TooManyPositionalArguments");
        }

        [Test]
        public void Should_find_invalid_invocation2() {
            const string editorText =
@"
public class TestClass {
    public void Test() {}
    public void Test() {}

    public void Test2() {
        this.Test();
    }
}
";

            var errors = GetErrors(editorText);

            Assert.AreEqual(1, errors.Length);
            errors[0].Message.ShouldEqual("Invocation error: AmbiguousMatch");
        }

        [Test]
        public void Should_find_invalid_invocation3() {
            const string editorText =
@"
public class TestClass {
    public void Test(string t) {}

    public void Test2() {
        this.Test(1);
    }
}
";

            var errors = GetErrors(editorText);

            Assert.AreEqual(2, errors.Length);
            errors[0].Message.ShouldEqual("Cannot convert from System.Int32 to System.String");
            errors[1].Message.ShouldEqual("Invocation error: ArgumentTypeMismatch");
        }

        private static Error[] GetErrors(string editorText)
        {
            var fileName = "test.cs";
            var solution = new FakeSolution();
            var project = new FakeProject();
            project.AddFile(editorText, fileName);
            solution.Projects.Add(project);

            var handler = new SemanticErrorsHandler(solution);
            var request = new Request
            {
                Buffer = editorText,
                FileName = fileName,
            };
            var semanticErrorsResponse = handler.FindSemanticErrors(request);
            return semanticErrorsResponse.Errors.ToArray();
        }
    }
}
