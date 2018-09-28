using System.Linq;
using System.Collections.Concurrent;
using ICSharpCode.NRefactory.Documentation;
using ICSharpCode.NRefactory.TypeSystem;
using MonoDevelop.Ide.TypeSystem;
using OmniSharp.Solution;

namespace OmniSharp.Documentation
{
    public static class XmlDocumentationProviderFactory
    {
        private static readonly ConcurrentDictionary<string, XmlDocumentationProvider> _providers =
            new ConcurrentDictionary<string, XmlDocumentationProvider>();


        public static IDocumentationProvider Get(IProject project, string assemblyName)
        {
            if (PlatformService.IsUnix)
                return new MonoDocDocumentationProvider();

            if (_providers.ContainsKey(assemblyName))
                return _providers[assemblyName];

            string fileName = null;
            IUnresolvedAssembly reference = project.References.OfType<IUnresolvedAssembly>().FirstOrDefault(i => i.AssemblyName.Equals(assemblyName));
            if (reference != null)
                fileName = XmlDocumentationProvider.LookupLocalizedXmlDoc(reference.Location);

            if (fileName != null)
            {
                var docProvider = new XmlDocumentationProvider(fileName);
                _providers.TryAdd(assemblyName, docProvider);
                return docProvider;
            }
            return null;
        }
    }
}

