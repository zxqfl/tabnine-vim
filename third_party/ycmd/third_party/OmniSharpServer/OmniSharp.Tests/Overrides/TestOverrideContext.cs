using System.Linq;
using NUnit.Framework;
using Should;
using OmniSharp.Common;
using OmniSharp.Overrides;
using OmniSharp.Parser;
using OmniSharp.Tests.Rename;
using OmniSharp.Configuration;

namespace OmniSharp.Tests.Overrides
{
    [TestFixture]
    public class OverridesTest
    {
        [Test]
        public void Should_not_offer_already_overridden_method()
        {
            var buffer = @"
public class WeirdString : String
{
    public override string ToString()
    {
        return ""weird"";
    }
$
}";
            var location = TestHelpers.GetLineAndColumnFromDollar(buffer);
            var request = new Request { Buffer = buffer, FileName = "myfile.cs", Line = location.Line, Column = location.Column };
            var solution = new FakeSolutionBuilder().AddFile(buffer, "myfile.cs").Build();
            var parser = new BufferParser(solution);
           
            var context = new OverrideContext(request, parser);
            var overrides = context.OverrideTargets.Select(m => m.OverrideTargetName).ToArray();
            overrides.ShouldNotContain("public virtual string ToString ();");

        }

        [Test]
        public void Should_insert_stub_method_override()
        {
            var buffer = 
@"
public class WeirdString : String
{
$
}";
            var location = TestHelpers.GetLineAndColumnFromDollar(buffer);
            buffer = buffer.Replace("$","");
            var request = new RunOverrideTargetRequest { Buffer = buffer, FileName = "myfile.cs", Line = location.Line, Column = location.Column };
            var solution = new FakeSolutionBuilder().AddFile(buffer, "myfile.cs").Build();
            var parser = new BufferParser(solution);
            var handler = new OverrideHandler (parser, new OmniSharpConfiguration ());
            request.OverrideTargetName = "public virtual string ToString ();";
            var response = handler.RunOverrideTarget (request);
            string expected = 
@"
public class WeirdString : String
{

    public override string ToString()
    {
        throw new System.NotImplementedException();
    }
}";
            string result = response.Buffer.Replace ("\r\n", "\n");
            result.ShouldEqual(expected);
        }
    }
}
