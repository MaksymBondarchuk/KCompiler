using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WebCompiler.Models
{
	public class Result
	{
		[DataMember] public OuterLexemes OuterLexemes { get; set; }
		[DataMember] public SyntaxResult SyntaxResult { get; set; }
		[DataMember] public PolishResultDto PolishResult { get; set; }
		[DataMember] public Guid ReferenceNumber { get; set; }
		[DataMember] public string Output { get; set; }
	}

	public class OuterLexemes
	{
		[DataMember] public List<Lt> Grammar { get; set; }
		[DataMember] public List<LexemeInCode> Lexemes { get; set; }
		[DataMember] public List<Constant> Constants { get; set; }
		[DataMember] public List<Identifier> Identifiers { get; set; }
		[DataMember] public List<LexicalError> Errors { get; set; }
	}

	public class Lt
	{
		public string Token { get; set; }
		public string[] Lexemes { get; set; }
	}
}