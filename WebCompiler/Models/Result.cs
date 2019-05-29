using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WebCompiler.Models
{
	public class Result
	{
		[DataMember] public OuterLexemes OuterLexemes { get; set; }
		[DataMember] public SyntaxResult SyntaxResult { get; set; }
		[DataMember] public PolishResult PolishResult { get; set; }
	}

	public class OuterLexemes
	{
		[DataMember] public List<LT> Grammar { get; set; }
		[DataMember] public List<LexemeInCode> Lexemes { get; set; }
		[DataMember] public List<Constant> Constants { get; set; }
		[DataMember] public List<Identifier> Identifiers { get; set; }
		[DataMember] public List<LexicalError> Errors { get; set; }
	}

	public class LT
	{
		public string Token { get; set; }
		public string[] Lexemes { get; set; }
	}

	public class PolishResult
	{
		[DataMember] public string ReversePolishNotation { get; set; }

		[DataMember] public IEnumerable<PolishTraceDto> Trace { get; set; }
	}
}