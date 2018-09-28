using System;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.TypeSystem;

namespace OmniSharp.AutoComplete
{
    public class MyCompletionsCategory : CompletionCategory
    {
		public MyCompletionsCategory(SymbolKind entityType)
        {
            DisplayText = GetVimKind(entityType);
        }

        public MyCompletionsCategory()
        {
            DisplayText = " ";
        }

		private string GetVimKind(SymbolKind entityType)
        {
    //        v	variable
    //f	function or method
    //m	member of a struct or class
            switch(entityType)
            {
			case(SymbolKind.Method):
                    return "f";
			case(SymbolKind.Field):
                    return "v";
			case(SymbolKind.Property):
                    return "m";
            }
            return " ";
        }

        public override int CompareTo(CompletionCategory other)
        {
            return String.CompareOrdinal(this.DisplayText, other.DisplayText);
        }
    }
}
