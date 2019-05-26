using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCompiler.Models
{
    public class SyntaxErrors
    {
        public int Line { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return $"{Line} {Text}";
        }
    }

    public class SyntaxResult
    {
        public bool Success { get; set; }
        public List<SyntaxErrors> Text { get; set; }
    }
}
