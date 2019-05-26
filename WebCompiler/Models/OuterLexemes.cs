using System;
using System.Collections;
using System.Collections.Generic;

namespace WebCompiler.Models
{
    public class Result
    {
        public OuterLexemes OuterLexemes { get; set; }
        public SyntaxResult SyntaxResult { get; set; }
    }
    public class OuterLexemes
    {
        public List<LT> Grammar { get; set; }
        public List<LexemInCode> Lexems { get; set; }
        public List<Constant> Constants { get; set; }
        public List<Identifier> Identifiers { get; set; }
        public List<LexicalError> Errors { get; set; }
    }

    public class LT
    {
        public string Token { get; set; }
        public string[] Lexemes { get; set; }

    }
}
