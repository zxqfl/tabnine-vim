using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using System.IO;
using ICSharpCode.NRefactory.CSharp.Refactoring;

namespace OmniSharp.AutoComplete
{
    public class ReturnTypeAmbience
    {
        public string ConvertSymbol(ISymbol symbol)
        {
            var stringWriter = new StringWriter();
            var astBuilder = new TypeSystemAstBuilder();
            astBuilder.AlwaysUseShortTypeNames = true;
            AstNode node = astBuilder.ConvertSymbol(symbol);
            var writer = new TextWriterTokenWriter (stringWriter);
            var rt = node.GetChildByRole (Roles.Type);
            if (!rt.IsNull)
            {
                rt.AcceptVisitor (new CSharpOutputVisitor (stringWriter, FormattingOptionsFactory.CreateMono()));

            }

            IProperty property = symbol as IProperty;
            if (property != null)
            {
                writer.Space ();
                writer.WriteToken (Roles.LBrace, "{");
                writer.Space ();
                if (property.CanGet)
                {
                    writer.WriteKeyword (PropertyDeclaration.GetKeywordRole, "get");
                    writer.WriteToken (Roles.Semicolon, ";");
                    writer.Space ();
                }
                if (property.CanSet)
                {
                    writer.WriteKeyword (PropertyDeclaration.SetKeywordRole, "set");
                    writer.WriteToken (Roles.Semicolon, ";");
                    writer.Space ();
                }
                writer.WriteToken (Roles.RBrace, "}");
            }
            return stringWriter.ToString();

            }
    }
}
