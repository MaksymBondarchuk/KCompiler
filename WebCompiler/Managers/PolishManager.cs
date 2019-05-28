using System.Collections.Generic;
using WebCompiler.Models;

namespace WebCompiler.Managers
{
	public class PolishManager
	{
		private int _i;
		
		public void Run(OuterLexemes lexemes)
		{
			_i = 2; // skip program <program name>
			ParseStatementsList(lexemes.Lexemes);
		}
		
		private void ParseStatementsList(List<LexemeInCode> lexemes)
		{
			bool? err;

			do
			{
				err = ParseStatement(lexemes);
				if (err.HasValue && err.Value == false)
				{
					return;
				}
			} while (err != null);
		}
		
		private bool? ParseStatement(List<LexemeInCode> lexemes)
		{
			switch (lexemes[_i].Token)
			{
				case "do":
					return ParseLoop(lexemes);
				case "if":
					return ParseConditional(lexemes);
				case "read":
					return ParseInput(lexemes);
				case "write":
					return ParseOutput(lexemes);
				case "var":
					return ParseDeclaration(lexemes);
				case "identifier":
					return ParseAssign(lexemes);
				default:
					return null;
			}
		}
	}
}