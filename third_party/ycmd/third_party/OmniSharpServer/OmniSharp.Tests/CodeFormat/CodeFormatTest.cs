using NUnit.Framework;
using OmniSharp.CodeFormat;
using OmniSharp.Configuration;
using Should;

namespace OmniSharp.Tests.CodeFormat
{
    [TestFixture]
    class CodeFormatTest
    {
        [Test]
        public void Should_format_code()
        {
            string code =
@"public class Test {
}";

            string expected =
@"public class Test
{
}";
           
            var handler = new CodeFormatHandler(new OmniSharpConfiguration());
            var buffer = handler.Format(new CodeFormatRequest {Buffer = code}).Buffer;
			buffer.Replace("\r\n", "\n").ShouldEqual(expected);
        }
    }
}
