using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCompiler.Models;

namespace WebCompiler.Managers
{
    public interface ICompilerManager
    {
        OuterLexemes LexicalAnalyzer(string text);
        SyntaxResult SyntaxAnalyzer(OuterLexemes lexemes);

    }
}
