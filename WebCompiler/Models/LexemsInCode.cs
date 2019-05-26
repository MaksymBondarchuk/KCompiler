using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCompiler.Models
{
    public class LexemInCode
    {
        public int LineNumber { get; set; }
        public string SubString { get; set; }
        public string Token { get; set; }
        public string Index { get; set; }

        public override string ToString()
        {
            return $"{SubString} ({Token})";
        }
    }
}
