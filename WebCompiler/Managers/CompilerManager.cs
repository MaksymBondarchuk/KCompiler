﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WebCompiler.Exceptions;
using WebCompiler.Models;

namespace WebCompiler.Managers
{
	public class CompilerManager : ICompilerManager
	{
		public OuterLexemes LexicalAnalyzer(string text)
		{
			return Parse(text);
		}

		public SyntaxResult SyntaxAnalyzer(OuterLexemes lexemes)
		{
			try
			{
				List<SyntaxError> res = SyntaxParse(lexemes);
				return new SyntaxResult
				{
					Success = !res?.Any() ?? true,
					Text = res
				};
			}
			catch (SyntaxException e)
			{
				return new SyntaxResult
				{
					Success = false,
					Text = new List<SyntaxError> {new SyntaxError {Text = e.Message}}
				};
			}
		}

		#region Lexical

		private static readonly string NewLine = Environment.NewLine;

		private List<string> PredefinedWords { get; } = new List<string>
		{
			"program", "do", "while", "enddo", "if", "then", "fi", "read", "write", "set", "var", "endprogram",
			"equals", "greaterthn", "lessthn"
		};

		private List<string> Delimiters { get; } = new List<string> {"+", "-", "*", "/", "(", ")", "¶", "\n", ","};
		private List<string> Operators { get; } = new List<string> {"+", "-", "*", "/", "(", ")"};
		private List<string> TrimDelimiters { get; } = new List<string> {" ", "\r", "\t", NewLine};

		public List<LexemeInCode> Lexemes { get; } = new List<LexemeInCode>();
		public List<Identifier> Identifiers { get; } = new List<Identifier>();
		public List<Constant> Constants { get; } = new List<Constant>();
		public List<LexicalError> Errors { get; } = new List<LexicalError>();

		public OuterLexemes Parse(string code)
		{
			int lineNumber = 1;
			var lexemeBuilder = new StringBuilder();
			foreach (string symbol in code.Select(c => c.ToString()))
			{
				int realLineNumber = lineNumber;
				if (symbol.Equals("\n"))
					lineNumber++;

				if (TrimDelimiters.Contains(symbol) || Delimiters.Contains(symbol))
				{
					string lexeme = lexemeBuilder.ToString();

					if (PredefinedWords.Contains(lexeme) || Operators.Contains(lexeme))
					{
						AddLexeme(lexeme, realLineNumber);
					}
					else if (IsIdentifier(lexeme))
					{
						// Duplicated identifier
						if (Lexemes.Any() && Lexemes.Last().SubString.Equals("var") &&
						    Identifiers.Any(e => e.Name.Equals(lexeme)))
						{
							AddError($"Duplicate declaration of {lexeme} identifier", realLineNumber);
							lexemeBuilder.Clear();
							continue;
						}

						// Usage of undeclared identifier
						if (Lexemes.Any() && !Lexemes.Last().SubString.Equals("var") &&
						    !Lexemes.Last().SubString.Equals("program") && !Identifiers.Any(e => e.Name.Equals(lexeme)))
						{
							AddError($"Usage of undeclared identifier: {lexeme}", realLineNumber);
							lexemeBuilder.Clear();
							continue;
						}

						AddIdentifier(lexeme);
						AddLexeme(lexeme, realLineNumber, IdentifierType.Identifier);
					}
					else if (IsConstant(lexeme))
					{
						AddConstant(lexeme);
						AddLexeme(lexeme, realLineNumber, IdentifierType.Constant);
					}
					else if (!string.IsNullOrEmpty(lexeme))
					{
						AddError($"Unknown lexeme: {lexeme}", realLineNumber);
						lexemeBuilder.Clear();
						continue;
					}

					if (Delimiters.Contains(symbol))
					{
						AddLexeme(symbol, realLineNumber);
					}

					lexemeBuilder.Clear();
					continue;
				}

				if (!TrimDelimiters.Contains(symbol))
					lexemeBuilder.Append(symbol);
			}

			return new OuterLexemes()
			{
				Identifiers = Identifiers,
				Constants = Constants,
				Lexemes = Lexemes,
				Errors = Errors,
				Grammar = GrammarLexemes
			};
		}

		private bool IsIdentifier(string lexeme)
		{
			return Regex.IsMatch(lexeme, "^@[a-zA-z]+$");
		}

		private bool IsConstant(string lexeme)
		{
			return Regex.IsMatch(lexeme, "^([0-9])+([.,][0-9]{1,8})?$");
		}

		private void AddIdentifier(string value)
		{
			if (Identifiers.Any(e => e.Name.Equals(value)))
			{
				return;
			}

			int index = Identifiers.Count + 1;
			Identifiers.Add(new Identifier
				{
					Index = index,
					Type = Lexemes.Any() ? Lexemes.Last()?.Token ?? "" : "",
					Name = value
				}
			);
		}

		private void AddConstant(string value)
		{
			if (Constants.Any(e => e.Name.Equals(value)))
			{
				return;
			}

			int index = Constants.Count + 1;
			Constants.Add(new Constant()
				{
					Index = index,
					Name = value
				}
			);
		}

		private void AddLexeme(string value, int line, IdentifierType type = IdentifierType.Unknown)
		{
			Lexemes.Add(new LexemeInCode
				{
					Index = GetIndex(type, value),
					SubString = value,
					LineNumber = line,
					Token = GetToken(value, type)
				}
			);
		}

		private string GetToken(string value, IdentifierType type)
		{
			if (type == IdentifierType.Identifier)
			{
				return "identifier";
			}

			if (type == IdentifierType.Constant && value.Contains("."))
			{
				return "float";
			}

			if (type == IdentifierType.Constant && !value.Contains("."))
			{
				return "integer";
			}

			return GrammarLexemes.First(e => e.Lexemes.Contains(value)).Token ?? "";
		}

		private void AddError(string value, int line)
		{
			Errors.Add(new LexicalError
			{
				Line = line,
				Text = value
			});
		}

		private string GetIndex(IdentifierType type, string value)
		{
			if (type == IdentifierType.Identifier)
			{
				return (Identifiers.IndexOf(Identifiers.Find(e => e.Name.Equals(value))) + 1).ToString();
			}

			if (type == IdentifierType.Constant)
			{
				return (Constants.IndexOf(Constants.Find(e => e.Name.Equals(value))) + 1).ToString();
			}

			return String.Empty;
		}

		private List<Lt> GrammarLexemes { get; } = new List<Lt>
		{
			new Lt()
			{
				Token = "program",
				Lexemes = new string[] {"program"},
			},

			new Lt()
			{
				Token = "endprogram",
				Lexemes = new string[] {"endprogram"},
			},
			new Lt()
			{
				Token = "do",
				Lexemes = new string[] {"do"},
			},
			new Lt()
			{
				Token = "while",
				Lexemes = new string[] {"while"},
			},
			new Lt()
			{
				Token = "enddo",
				Lexemes = new string[] {"enddo"},
			},
			new Lt()
			{
				Token = "if",
				Lexemes = new string[] {"if"},
			},
			new Lt()
			{
				Token = "then",
				Lexemes = new string[] {"then"},
			},
			new Lt()
			{
				Token = "fi",
				Lexemes = new string[] {"fi"}
			},
			new Lt()
			{
				Token = "read",
				Lexemes = new string[] {"read"},
			},
			new Lt()
			{
				Token = "write",
				Lexemes = new string[] {"write"},
			},
			new Lt()
			{
				Token = "set",
				Lexemes = new string[] {"set"},
			},
			new Lt()
			{
				Token = "var",
				Lexemes = new string[] {"var"},
			},
			new Lt()
			{
				Token = "equals",
				Lexemes = new string[] {"equals"},
			},
			new Lt()
			{
				Token = "greaterthn",
				Lexemes = new string[] {"greaterthn"},
			},
			new Lt()
			{
				Token = "lessthn",
				Lexemes = new string[] {"lessthn"},
			},

			new Lt()
			{
				Token = "operation",
				Lexemes = new string[] {"+", "-", "*", "/"},
			},

			new Lt()
			{
				Token = "parentheses",
				Lexemes = new string[] {"(", ")"},
			},

			new Lt()
			{
				Token = "ws",
				Lexemes = new string[] {"' '"},
			},

			new Lt()
			{
				Token = "delimiter",
				Lexemes = new string[] {"\n"},
			},
			new Lt()
			{
				Token = "identifier",
				Lexemes = new string[] {"@r1", "@1r", "@var"},
			},
			new Lt()
			{
				Token = "integer",
				Lexemes = new string[] {"2", "-12"},
			},
			new Lt()
			{
				Token = "float",
				Lexemes = new string[] {"2.0", "-12.12"},
			}
		};

		#endregion

		#region Syntax

		private List<SyntaxError> SyntaxErr = new List<SyntaxError>();

		private List<SyntaxError> SyntaxParse(OuterLexemes lexemes)
		{
			int i = 0;
			if (lexemes.Errors.Any())
			{
				SyntaxErr.Add(new SyntaxError
				{
					Line = 0,
					Text = "Lexical errors detected"
				});
				return SyntaxErr;
			}

			ParseProgram(lexemes.Lexemes, ref i);


			return SyntaxErr;
		}

		private bool ParseProgram(List<LexemeInCode> lexemes, ref int i)
		{
			if (!lexemes[i].Token.Equals("program"))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text = $"Program must start with the 'program' keyword in line {lexemes[i].LineNumber}"
					});
				return false;
			}

			i++;

			if (!lexemes[i].Token.Equals("identifier"))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text =
							$"Program name identifier is expected, seen {lexemes[i].SubString} in line {lexemes[i].LineNumber}"
					});
				return false;
			}

			i++;

			if (lexemes[i].Token.Equals("delimiter"))
			{
				i++;
			}

			if (!ParseStatementsList(lexemes, ref i))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text = $"Error in parsing StatementList in line {lexemes[i].LineNumber}"
					});
				return false;
			}

			//if (lexemes[i].Token.Equals("delimiter"))
			//{
			//    i++;
			//}

			if (!lexemes[i].Token.Equals("endprogram"))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text = $"Program must end with the 'endprogram' keyword"
					});
				return false;
			}

			int count = lexemes.Count - 1 - i;
			List<LexemeInCode> diff = lexemes.GetRange(i + 1, count);
			if (i != lexemes.Count - 1 && diff.Any(e => !e.Token.Equals("delimiter")))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text =
							$"Smth was found in the end of the program: {lexemes[i].SubString} in line {lexemes[i].LineNumber}"
					});
				return false;
			}

			return true;
		}

		private bool ParseStatementsList(List<LexemeInCode> lexemes, ref int i)
		{
			bool? err;

			do
			{
				err = ParseStatement(lexemes, ref i);
				if (err.HasValue && err.Value == false)
					return false;
			} while (err != null);

			return true;
		}

		private bool? ParseStatement(List<LexemeInCode> lexemes, ref int i)
		{
			switch (lexemes[i].Token)
			{
				case "do":
					return ParseLoop(lexemes, ref i);
				case "if":
					return ParseConditional(lexemes, ref i);
				case "read":
					return ParseInput(lexemes, ref i);
				case "write":
					return ParseOutput(lexemes, ref i);
				case "var":
					return ParseDeclaration(lexemes, ref i);
				case "identifier":
					return ParseAssign(lexemes, ref i);
				default:
					return null;
			}
		}

		private bool ParseDelimiter(List<LexemeInCode> lexemes, ref int i)
		{
			if (!lexemes[i].Token.Equals("delimiter"))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text = $"Delimiter expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
					});
				return false;
			}

			i++;
			return true;
		}

		private bool ParseLoop(List<LexemeInCode> lexemes, ref int i)
		{
			i++;

			if (!lexemes[i].Token.Equals("while"))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text =
							$"'while' keyword expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
					});
				return false;
			}

			i++;

			if (lexemes[i].Token.Equals("delimiter"))
			{
				i++;
			}

			if (!lexemes[i].SubString.Equals("("))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text = $"'(' expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
					});
				return false;
			}

			i++;

			if (!ParseLogicalExpression(lexemes, ref i))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text = $"Error parsing Logical expression in line {lexemes[i].LineNumber}"
					});
				return false;
			}


			if (!lexemes[i].SubString.Equals(")"))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text = $"')' expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
					});
				return false;
			}

			i++;

			if (lexemes[i].Token.Equals("delimiter"))
			{
				i++;
			}

			if (!ParseStatementsList(lexemes, ref i))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text = $"Error parsing StatementList in line {lexemes[i].LineNumber}"
					});
				return false;
			}

			if (lexemes[i].Token.Equals("delimiter"))
			{
				i++;
			}

			if (!lexemes[i].SubString.Equals("enddo"))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text =
							$"'enddo' keyword expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
					});
				return false;
			}

			i++;

			if (!ParseDelimiter(lexemes, ref i)) return false;

			return true;
		}

		private bool ParseConditional(List<LexemeInCode> lexemes, ref int i)
		{
			i++;

			if (lexemes[i].Token.Equals("delimiter"))
			{
				i++;
			}

//			if (!lexemes[i].SubString.Equals("("))
//			{
//				SyntaxErr.Add(
//					new SyntaxError
//					{
//						Text = $"'(' expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
//					});
//				return false;
//			}
//
//			i++;

			if (!ParseLogicalExpression(lexemes, ref i)) return false;

//			if (!lexemes[i].SubString.Equals(")"))
//			{
//				SyntaxErr.Add(
//					new SyntaxError
//					{
//						Text = $"')' expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
//					});
//				return false;
//			}
//
//			i++;

			if (lexemes[i].Token.Equals("delimiter"))
			{
				i++;
			}

			if (!lexemes[i].SubString.Equals("then"))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text =
							$"'then' keyword expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
					});
				return false;
			}

			i++;

			if (lexemes[i].Token.Equals("delimiter"))
			{
				i++;
			}

			if (!ParseStatementsList(lexemes, ref i))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text = $"Error parsing StatementList in line {lexemes[i].LineNumber}"
					});
				return false;
			}

			if (lexemes[i].Token.Equals("delimiter"))
			{
				i++;
			}

			if (!lexemes[i].SubString.Equals("fi"))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text =
							$"'fi' keyword expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
					});
				return false;
			}

			i++;

			return ParseDelimiter(lexemes, ref i);
		}

		private bool ParseInput(List<LexemeInCode> lexemes, ref int i)
		{
			i++;


			if (!lexemes[i].SubString.Equals("("))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text = $"'(' expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
					});
				return false;
			}

			i++;

			if (!lexemes[i].Token.Equals("identifier"))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text =
							$"Identifier expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
					});
				return false;
			}

			i++;

			if (!lexemes[i].SubString.Equals(")"))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text = $"')' expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
					});
				return false;
			}

			i++;


			if (!ParseDelimiter(lexemes, ref i)) return false;

			return true;
		}

		private bool ParseOutput(List<LexemeInCode> lexemes, ref int i)
		{
			i++;


			if (!lexemes[i].SubString.Equals("("))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text = $"'(' expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
					});
				return false;
			}

			i++;

			if (!lexemes[i].Token.Equals("identifier"))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text =
							$"Identifier expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
					});
				return false;
			}

			i++;

			if (!lexemes[i].SubString.Equals(")"))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text = $"')' expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
					});
				return false;
			}

			i++;


			if (!ParseDelimiter(lexemes, ref i)) return false;

			return true;
		}

		private bool ParseDeclaration(List<LexemeInCode> lexemes, ref int i)
		{
			i++;

			if (!lexemes[i].Token.Equals("identifier"))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text =
							$"Identifier expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
					});
				return false;
			}

			i++;

			if (lexemes[i].Token.Equals("set"))
			{
				i++;
				if (!ParseArithmeticExpression(lexemes, ref i))
				{
					SyntaxErr.Add(
						new SyntaxError
						{
							Text =
								$"'set' keyword expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
						});
					return false;
				}
			}

			if (!ParseDelimiter(lexemes, ref i)) return false;

			return true;
		}

		private bool ParseAssign(List<LexemeInCode> lexemes, ref int i)
		{
			i++;
			if (!lexemes[i].Token.Equals("set"))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text =
							$"'set' keyword expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
					});
				return false;
			}

			i++;

			if (!ParseArithmeticExpression(lexemes, ref i))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text = $"Error during parsing Arithmetic Expression in line {lexemes[i].LineNumber}"
					});
				return false;
			}

			if (!ParseDelimiter(lexemes, ref i)) return false;

			return true;
		}

		private bool ParseLogicalExpression(List<LexemeInCode> lexemes, ref int i)
		{
			if (!ParseArithmeticExpression(lexemes, ref i))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text = $"Error during parsing Arithmetic Expression in line {lexemes[i].LineNumber}"
					});
				return false;
			}

			if (!lexemes[i].Token.Equals("equals") && !lexemes[i].Token.Equals("greaterthn") &&
			    !lexemes[i].Token.Equals("lessthn"))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text =
							$"'equals' or 'greaterthn' or 'lessthn' keyword expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
					});
				return false;
			}

			i++;

			if (!ParseArithmeticExpression(lexemes, ref i))
			{
				SyntaxErr.Add(
					new SyntaxError
					{
						Text = $"Error during parsing Arithmetic Expression in line {lexemes[i].LineNumber}"
					});
				return false;
			}

			return true;
		}

		private bool ParseArithmeticExpression(List<LexemeInCode> lexemes, ref int i)
		{
			#region <sign> <arithmetic leaf>

			{
				bool isSign = ParseSign(lexemes, ref i);
				bool isArithmeticLeaf = ParseArithmeticLeaf(lexemes, ref i);
				if (isSign && !isArithmeticLeaf)
				{
					throw new SyntaxException(
						$"Identifier or literal expected on line {lexemes[i].LineNumber}, instead seen {lexemes[i].SubString}");
				}

				#region <arithmetic expression> <operation> <arithmetic expression>
				bool isOperation = ParseOperation(lexemes, ref i);
				if (isArithmeticLeaf && !isOperation)
				{
					if (lexemes[i].SubString.Equals(")")
					    || lexemes[i].SubString.Equals("\n")
					    || lexemes[i].SubString.Equals("equals")
					    || lexemes[i].SubString.Equals("greaterthn")
					    || lexemes[i].SubString.Equals("lessthn"))
					{
						return true;
					}

					throw new SyntaxException(
						$"Operation expected on line {lexemes[i].LineNumber}, instead seen {lexemes[i].SubString}");
				}

				if (isArithmeticLeaf)
				{
					return ParseArithmeticExpression(lexemes, ref i);
				}
				#endregion
			}

			#endregion

			#region (<arithmetic expression>)

			bool isParenthesis = lexemes[i].SubString.Equals("(");
			if (isParenthesis)
			{
				i++;

				if (!ParseArithmeticExpression(lexemes, ref i))
				{
					throw new SyntaxException($"Incorrect Arithmetic Expression");
				}

				if (!lexemes[i].SubString.Equals(")"))
				{
					throw new SyntaxException($"Expecting ')' in line {lexemes[i].LineNumber}, instead seen {lexemes[i].SubString}");
				}
				i++;

				bool isOperation = ParseOperation(lexemes, ref i);
				if (isOperation)
				{
					ParseArithmeticExpression(lexemes, ref i);
				}
				return true;
			}

			#endregion

			throw new SyntaxException($"Error during parsing Arithmetic Expression in line {lexemes[i].LineNumber}");
		}


		private bool ParseSign(List<LexemeInCode> lexemes, ref int i)
		{
			if (lexemes[i].SubString.Equals("+") || lexemes[i].SubString.Equals("-"))
			{
				i++;
				return true;
			}

			return false;
		}

		private bool ParseArithmeticLeaf(List<LexemeInCode> lexemes, ref int i)
		{
			if (lexemes[i].Token.Equals("float") || lexemes[i].Token.Equals("integer") ||
			    lexemes[i].Token.Equals("identifier"))
			{
				i++;
				return true;
			}

			return false;
		}

		private bool ParseArthExpressionWithOperation(List<LexemeInCode> lexemes, ref int i)
		{
			if (!ParseArithmeticExpression(lexemes, ref i)) return false;
			if (!ParseOperation(lexemes, ref i)) return false;
			if (!ParseArithmeticExpression(lexemes, ref i)) return false;

			return true;
		}

		private bool ParseOperation(List<LexemeInCode> lexemes, ref int i)
		{
			if (!lexemes[i].SubString.Equals("+") && !lexemes[i].SubString.Equals("-") &&
			    !lexemes[i].SubString.Equals("*") && !lexemes[i].SubString.Equals("/"))
			{
//				SyntaxErr.Add(new SyntaxErrors
//				{
//					Text = $"Math operation sign expected in line {lexemes[i].LineNumber} instead seen {lexemes[i].SubString}"
//				});
				return false;
			}

			i++;
			return true;
		}

		private bool ParseOperationLight(List<LexemeInCode> lexemes, ref int i)
		{
			if (!lexemes[i].SubString.Equals("+") && !lexemes[i].SubString.Equals("-") &&
			    !lexemes[i].SubString.Equals("*") & !lexemes[i].SubString.Equals("/"))
			{
				return false;
			}

			i++;
			return true;
		}

		#endregion
	}
}