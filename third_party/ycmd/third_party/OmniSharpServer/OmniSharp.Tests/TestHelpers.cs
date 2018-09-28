using ICSharpCode.NRefactory;
using System;

namespace OmniSharp.Tests
{
    public static class TestHelpers
    {
        public static TextLocation GetLineAndColumnFromDollar(string text)
        {
            var indexOfDollar = text.IndexOf("$");
            
            if (indexOfDollar == -1)
                throw new ArgumentException("Expected a $ sign in test input");

            return GetLineAndColumnFromIndex(text, indexOfDollar);
        }

        public static TextLocation GetLineAndColumnFromIndex(string text, int index)
        {
            int lineCount = 1, lastLineEnd = -1;
            for (int i = 0; i < index; i++)
                if (text[i] == '\n')
                {
                    lineCount++;
                    lastLineEnd = i;
                }

            return new TextLocation(lineCount, index - lastLineEnd);
        }
    }
}
