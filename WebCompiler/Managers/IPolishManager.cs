using WebCompiler.Models;

namespace WebCompiler.Managers
{
    public interface IPolishManager
    {
        PolishResult Run(OuterLexemes lexemes);
    }
}