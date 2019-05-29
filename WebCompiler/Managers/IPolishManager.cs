using System.Collections.Generic;
using WebCompiler.Models;

namespace WebCompiler.Managers
{
    public interface IPolishManager
    {
        List<PolishNotation> ReversePolishNotation { get; }
        
        List<PolishTrace> Trace { get; }
        
        void Run(OuterLexemes lexemes);
    }
}