using WebCompiler.Models;

namespace WebCompiler.Managers
{
    public interface ICompilerManager
    {
        OuterLexemes LexicalAnalyzer(string text);

        SyntaxResult SyntaxAnalyzer(OuterLexemes lexemes);
    }
}
