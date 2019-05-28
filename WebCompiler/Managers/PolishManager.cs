using System;
using System.Collections.Generic;
using WebCompiler.Models;

namespace WebCompiler.Managers
{
	public class PolishManager : IPolishManager
	{
		public List<PolishNotation> ReversePolishNotation { get; } = new List<PolishNotation>();

		private int _i;
		private OuterLexemes _outerLexemes;

		private Stack<PolishNotation> Stack { get; } = new Stack<PolishNotation>();

		public void Run(OuterLexemes lexemes)
		{
			_i = 3; // skip program <program name> & delimiter
			_outerLexemes = lexemes;

			ReversePolishNotation.Clear();
			ParseStatementsList();
		}

		private void ParseStatementsList()
		{
			bool? err;

			do
			{
				err = ParseStatement();
				if (err.HasValue && err.Value == false)
				{
					return;
				}
			} while (err != null);
		}

		private bool? ParseStatement()
		{
			switch (_outerLexemes.Lexemes[_i].Token)
			{
//                case "do":
//                    return ParseLoop(lexemes);
//                case "if":
//                    return ParseConditional(lexemes);
				case "read":
					ParseInput();
					return true;
//                case "write":
//                    return ParseOutput(lexemes);
				case "var":
					ParseDeclaration();
					return true;
//                case "identifier":
//                    return ParseAssign(lexemes);
				default:
					return null;
			}
		}

		private void ParseDeclaration()
		{
			_i++;
			// Identifier
			ReversePolishNotation.Add(new PolishNotation
			{
				Token = _outerLexemes.Lexemes[_i].SubString,
				Type = PolishNotationTokenType.Identifier
			});

			_i++;

			if (_outerLexemes.Lexemes[_i].Token.Equals("set"))
			{
				_i++;
				ParseArithmeticExpression();
			}

			_i++; // Skip delimiter
		}

		private void ParseInput()
		{
			_i += 2; // skip "("

			// Identifier
			ReversePolishNotation.Add(new PolishNotation
			{
				Token = _outerLexemes.Lexemes[_i].SubString,
				Type = PolishNotationTokenType.Identifier
			});

			// "read"
			ReversePolishNotation.Add(new PolishNotation
			{
				Token = "read",
				Type = PolishNotationTokenType.Operator
			});
			
			_i += 2; // skip ")" and delimiter
		}

		private void ParseArithmeticExpression()
		{
			throw new NotImplementedException();
		}
	}
}