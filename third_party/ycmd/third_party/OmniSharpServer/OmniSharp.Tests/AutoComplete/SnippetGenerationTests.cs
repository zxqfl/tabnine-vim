using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using NUnit.Framework;
using OmniSharp.AutoComplete;

namespace OmniSharp.Tests.AutoComplete
{
    [TestFixture]
    public class SnippetGenerationTests
    {
        IUnresolvedAssembly mscorlib;
        IUnresolvedAssembly myLib;
        ICompilation compilation;
        SnippetGenerator snippetGenerator;

        [SetUp]
        public void SetUp()
        {
            snippetGenerator = new SnippetGenerator(true);
            var loader = new CecilLoader();
            loader.IncludeInternalMembers = true;
            myLib = loader.LoadAssemblyFile(typeof(SnippetGenerationTests).Assembly.Location);
            mscorlib = loader.LoadAssemblyFile(typeof(object).Assembly.Location);
            compilation = new SimpleCompilation(myLib, mscorlib);
        }

        [Test]
        public void GenericType()
        {
            var typeDef = compilation.FindType(typeof(Dictionary<,>)).GetDefinition();
            string result = snippetGenerator.Generate(typeDef);
            result.ShouldEqual("Dictionary<${1:TKey}, ${2:TValue}>()$0");
        }

        [Test]
        public void FullDelegate()
        {
            var func = compilation.FindType(typeof(Func<,>)).GetDefinition();
            snippetGenerator.ConversionFlags = ConversionFlags.All;
            Assert.AreEqual("Func<${1:in T}, ${2:out TResult}>(${3:T arg})$0", snippetGenerator.Generate(func));
        }

        [Test]
        public void GenericTypeWithNested()
        {
            var typeDef = compilation.FindType(typeof(List<>)).GetDefinition();
            snippetGenerator.ConversionFlags = ConversionFlags.UseFullyQualifiedEntityNames | ConversionFlags.ShowTypeParameterList;
            string result = snippetGenerator.Generate(typeDef);
            Assert.AreEqual("List<${1:T}>()$0", result);
        }

        [Test]
        public void AutomaticProperty()
        {
            var prop = compilation.FindType(typeof(SnippetGenerationTests.Program)).GetProperties(p => p.Name == "Test").Single();
            snippetGenerator.ConversionFlags = ConversionFlags.StandardConversionFlags;
            string result = snippetGenerator.Generate(prop);

            Assert.AreEqual("Test$0", result);
        }

        class Program
        {
            public int Test { get; set; }
        }
    }
}