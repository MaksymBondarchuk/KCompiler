using System.Collections.Generic;
using WebCompiler.Models;

namespace WebCompiler.Managers
{
    public interface IPolishManager
    {
        List<PolishNotation> ReversePolishNotation { get; }
        
        void Run(OuterLexemes lexemes);
    }
}