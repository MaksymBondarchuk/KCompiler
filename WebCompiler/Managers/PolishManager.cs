using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using WebCompiler.Models;

namespace WebCompiler.Managers
{
	public class PolishManager : IPolishManager
	{
		private List<PolishNotation> ReversePolishNotation { get; } = new List<PolishNotation>();
		private List<PolishTrace> Trace { get; } = new List<PolishTrace>();

		private int _i;
		private OuterLexemes _outerLexemes;

		private int _labelNumber;

		// Name, Address
		private readonly Dictionary<string, int> _labelAddresses = new Dictionary<string, int>();

		private readonly Dictionary<string, int> _operatorsPriorities = new Dictionary<string, int>
		{
			{"do", 1},
			{"while", 1},
			{"enddo", 1},
			{"if", 1},
			{"then", 1},
			{"fi", 1},
			{"read", 2},
			{"write", 2},
			{"(", 3},
			{")", 3},
			{"var", 2},
			{"set", 2},
			{"equals", 3},
			{"greaterthn", 3},
			{"lessthn", 3},
			{"+", 4},
			{"-", 4},
			{"*", 5},
			{"/", 5},
			{"@+", 6},
			{"@-", 6}
		};

		private Stack<PolishNotation> Stack { get; } = new Stack<PolishNotation>();

		public PolishResult Run(OuterLexemes lexemes)
		{
			_i = 3; // skip program <program name> & delimiter
			_outerLexemes = lexemes;
			_labelNumber = 0;
			_labelAddresses.Clear();
			ReversePolishNotation.Clear();

			ParseStatementsList();

			return new PolishResult
			{
				ReversePolishNotation = ReversePolishNotation,
				Trace = Trace,
				LabelAddresses = _labelAddresses
			};
		}

		private void ParseStatementsList()
		{
			do
			{
				ParseStatement();
			} while (_outerLexemes.Lexemes[_i].Token == "do"
			         || _outerLexemes.Lexemes[_i].Token == "if"
			         || _outerLexemes.Lexemes[_i].Token == "read"
			         || _outerLexemes.Lexemes[_i].Token == "write"
			         || _outerLexemes.Lexemes[_i].Token == "var"
			         || _outerLexemes.Lexemes[_i].Token == "identifier");
		}

		private void ParseStatement()
		{
			switch (_outerLexemes.Lexemes[_i].Token)
			{
				case "do":
					ParseLoop();
					break;
				case "if":
					ParseConditional();
					break;
				case "read":
					ParseInput();
					break;
				case "write":
					ParseOutput();
					break;
				case "var":
					ParseDeclaration();
					break;
				case "identifier":
					ParseAssign();
					break;
			}

			DijkstraStep("\\n", PolishNotationTokenType.Delimiter);
		}

		private void ParseDeclaration()
		{
			// "var"
			DijkstraStep("var", PolishNotationTokenType.Operator);

			// <identifier>
			DijkstraStep(_outerLexemes.Lexemes[_i].SubString,
				PolishNotationTokenType.Identifier,
				_outerLexemes.Lexemes[_i].Token.Equals("set"));

			// "set" (optional)
			if (_outerLexemes.Lexemes[_i].Token.Equals("set"))
			{
				DijkstraStep("set", PolishNotationTokenType.Operator);

				// <arithmetic expression>
				ParseArithmeticExpression();
			}
		}

		private void ParseAssign()
		{
			// <identifier>
			DijkstraStep(_outerLexemes.Lexemes[_i].SubString, PolishNotationTokenType.Identifier, true);

			// "set"
			DijkstraStep("set", PolishNotationTokenType.Operator);

			// <arithmetic expression>
			ParseArithmeticExpression();
		}

		private void ParseInput()
		{
			// "read"
			DijkstraStep("read", PolishNotationTokenType.Operator);

			// "("
			MoveNext(); // Skip

			// Identifier
			DijkstraStep(_outerLexemes.Lexemes[_i].SubString, PolishNotationTokenType.Identifier, true);

			// ")"
			MoveNext(); // Skip
		}

		private void ParseOutput()
		{
			// "write"
			DijkstraStep("write", PolishNotationTokenType.Operator);

			// "("
			MoveNext(); // Skip

			// Identifier
			DijkstraStep(_outerLexemes.Lexemes[_i].SubString, PolishNotationTokenType.Identifier);

			// ")"
			MoveNext(); // Skip
		}

		private void ParseArithmeticExpression()
		{
			do
			{
				// Any of the following can be in arithmetic expression
				ParseOperation(); // !!! Must be before Sign ("+" and "-" are both Sign and Operation) - we check difference in Operation
				ParseSign();
				ParseArithmeticLeaf();

				if (_outerLexemes.Lexemes[_i].SubString.Equals("(") || _outerLexemes.Lexemes[_i].SubString.Equals(")"))
				{
					DijkstraStep(_outerLexemes.Lexemes[_i].SubString, PolishNotationTokenType.Operator);
				}
			} while (_outerLexemes.Lexemes[_i].Token != "delimiter" &&
			         _outerLexemes.Lexemes[_i].Token != "equals" &&
			         _outerLexemes.Lexemes[_i].Token != "greaterthn" &&
			         _outerLexemes.Lexemes[_i].Token != "lessthn");
		}

		private void ParseSign()
		{
			if (_outerLexemes.Lexemes[_i].SubString.Equals("+") || _outerLexemes.Lexemes[_i].SubString.Equals("-"))
			{
				DijkstraStep($"@{_outerLexemes.Lexemes[_i].SubString}", PolishNotationTokenType.Operator);
			}
		}

		private void ParseArithmeticLeaf()
		{
			if (_outerLexemes.Lexemes[_i].Token.Equals("float")
			    || _outerLexemes.Lexemes[_i].Token.Equals("integer"))
			{
				DijkstraStep(_outerLexemes.Lexemes[_i].SubString, PolishNotationTokenType.Literal);
			}
			if (_outerLexemes.Lexemes[_i].Token.Equals("identifier"))
			{
				DijkstraStep(_outerLexemes.Lexemes[_i].SubString, PolishNotationTokenType.Identifier);
			}
		}

		private void ParseOperation()
		{
			LexemeInCode previousLexeme = _outerLexemes.Lexemes[_i - 1];
			if (previousLexeme.Token != "identifier" &&
			    previousLexeme.Token != "integer" &&
			    previousLexeme.Token != "float" &&
			    previousLexeme.SubString != ")")
			{
				return;
			}

			if (_outerLexemes.Lexemes[_i].SubString.Equals("+")
			    || _outerLexemes.Lexemes[_i].SubString.Equals("-")
			    || _outerLexemes.Lexemes[_i].SubString.Equals("*")
			    || _outerLexemes.Lexemes[_i].SubString.Equals("/"))
			{
				DijkstraStep(_outerLexemes.Lexemes[_i].SubString, PolishNotationTokenType.Operator);
			}
		}

		private void ParseConditional()
		{
			// "if"
			DijkstraStep("if", PolishNotationTokenType.If);

			// <logical expression>
			ParseLogicalExpression();

			// "then"
			DijkstraStep("then", PolishNotationTokenType.Then);
			ParseDelimiterSilent();

			// <operators list>
			ParseStatementsList();

			// "fi"
			DijkstraStep("fi", PolishNotationTokenType.Fi);
		}

		private void ParseLogicalExpression()
		{
			// <arithmetic expression>
			ParseArithmeticExpression();

			// "equals" or "greaterthn" or "lessthn"
			DijkstraStep(_outerLexemes.Lexemes[_i].Token, PolishNotationTokenType.Operator);

			// <arithmetic expression>
			ParseArithmeticExpression();
		}

		private void ParseDelimiterSilent()
		{
			if (!_outerLexemes.Lexemes[_i].Token.Equals("delimiter"))
			{
				MoveNext();
			}
		}

		private void ParseLoop()
		{
			// Skip "do"
			MoveNext();

			// "while"
			DijkstraStep("while", PolishNotationTokenType.While);

			// Skip "("
			MoveNext();

			// <logical expression>
			ParseLogicalExpression();

			// ")"
			DijkstraStep(_outerLexemes.Lexemes[_i].Token, PolishNotationTokenType.TechnicalDo);

			// <operators list>
			ParseStatementsList();

			// "enddo"
			DijkstraStep("enddo", PolishNotationTokenType.Enddo);
		}

		#region Dijkstra

		[SuppressMessage("ReSharper", "CommentTypo")]
		private void DijkstraStep(string input, PolishNotationTokenType type, bool isAssignmentToThisIdentifier = false)
		{
			switch (type)
			{
				// 1.	Ідентифікатори та константи проходять від входу прямо до виходу
				case PolishNotationTokenType.Identifier:
				case PolishNotationTokenType.Literal:
					ReversePolishNotation.Add(new PolishNotation
					{
						Token = input, Type = type, IsAssignmentToThisIdentifier = isAssignmentToThisIdentifier
					});
					break;
				// Операції звичайно потрапляють на вихід через магазин
				case PolishNotationTokenType.Operator:
					AddOperatorToStack(input);
					break;
				// 4.	Якщо вхідний ланцюжок порожній, всі операції зі стека по черзі передаються на вихід
				// 		за ознакою кінця виразу (наприклад, ";").
				case PolishNotationTokenType.Delimiter:
				{
					while (Stack.Count != 0 &&
					       !Stack.Peek().Token.StartsWith("if") &&
					       !Stack.Peek().Token.StartsWith("while"))
					{
						ReversePolishNotation.Add(Stack.Pop());
					}

					ReversePolishNotation.Add(new PolishNotation {Token = input, Type = type});
				}
					break;
				case PolishNotationTokenType.If:
					Stack.Push(new PolishNotation {Token = input, Type = type});
					break;
				// Символ then з пріоритетом 1 виштовхує зі стека всі знаки до першого
				// if виключно. При цьому генерується нова мітка mi й у вихідний рядок
				// заноситься mi УПХ. Потім мітка mi заноситься в таблицю міток і дописується у вершину стека,
				// таким чином, запис if у вершині стека перетворюється на запис if mi
				case PolishNotationTokenType.Then:
				{
					PolishNotation head = Stack.Peek();
					while (!head.Token.StartsWith("if"))
					{
						ReversePolishNotation.Add(Stack.Pop());
						head = Stack.Peek();
					}

					string label = GenerateLabel();
					head.Token = $"{head.Token} {label}";
					ReversePolishNotation.Add(new PolishNotation {Token = $"{label}", Type = type});
					ReversePolishNotation.Add(new PolishNotation {Token = "УПХ", Type = type});
				}
					break;
				// Символ кінця умовного оператора (наприклад, ; або end) виштовхує зі
				// стека все до найближчого if. Це може бути if mi mi+1 (або if mi у разі
				// конструкції if <вираз> then <оператор>;). Запис видаляється зі стека,
				// а у вихідний рядок додається mi+1: (або mi: у випадку неповного оператора умовного переходу).
				case PolishNotationTokenType.Fi:
				{
					PolishNotation head = Stack.Peek();
					while (!head.Token.StartsWith("if"))
					{
						ReversePolishNotation.Add(Stack.Pop());
						head = Stack.Peek();
					}

					head = Stack.Pop();
					string label = head.Token.Split(" ").Last();
					ReversePolishNotation.Add(new PolishNotation {Token = $"{label}", Type = type});
					ReversePolishNotation.Add(new PolishNotation {Token = $":", Type = type});
				}
					break;
				case PolishNotationTokenType.While:
				{
					string label = GenerateLabel();
					Stack.Push(new PolishNotation {Token = $"{input} {label}", Type = type});
					ReversePolishNotation.Add(new PolishNotation {Token = $"{label}", Type = type});
					ReversePolishNotation.Add(new PolishNotation {Token = $":", Type = type});
				}
					break;
				case PolishNotationTokenType.TechnicalDo:
				{
					PolishNotation head = Stack.Peek();
					while (!head.Token.StartsWith("while"))
					{
						ReversePolishNotation.Add(Stack.Pop());
						head = Stack.Peek();
					}

					string label = GenerateLabel();
					head.Token = $"{head.Token} {label}";
					ReversePolishNotation.Add(new PolishNotation {Token = $"{label}", Type = type});
					ReversePolishNotation.Add(new PolishNotation {Token = "УПХ", Type = type});
				}
					break;
				case PolishNotationTokenType.Enddo:
				{
					PolishNotation head = Stack.Peek();
					while (!head.Token.StartsWith("while"))
					{
						ReversePolishNotation.Add(Stack.Pop());
						head = Stack.Peek();
					}

					head = Stack.Pop();
					string[] labels = head.Token.Split(" ");
					ReversePolishNotation.Add(new PolishNotation {Token = $"{labels[1]}", Type = type});
					ReversePolishNotation.Add(new PolishNotation {Token = "БП", Type = type});
					ReversePolishNotation.Add(new PolishNotation {Token = $"{labels.Last()}", Type = type});
					ReversePolishNotation.Add(new PolishNotation {Token = $":", Type = type});
				}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}

			Trace.Add(new PolishTrace
			{
				Input = input,
				Stack = new Stack<PolishNotation>(Stack),
				ReversePolishNotation = new List<PolishNotation>(ReversePolishNotation)
			});
			MoveNext();
		}

		[SuppressMessage("ReSharper", "CommentTypo")]
		private void AddOperatorToStack(string @operator)
		{
			// Відкриваюча дужка "(" повинна мати найнижчий пріоритет, але записуватися в стек, нічого не виштовхуючи
			if (@operator == "(")
			{
				Stack.Push(new PolishNotation {Token = @operator, Type = PolishNotationTokenType.Operator});
				return;
			}

			// 3.	Якщо стек порожній, то поточна операція заноситься в стек
			if (Stack.Count == 0)
			{
				Stack.Push(new PolishNotation {Token = @operator, Type = PolishNotationTokenType.Operator});
				return;
			}

			PolishNotation head = Stack.Peek();
			string headToken = head.Token.StartsWith("if")
				? "if"
				: head.Token.StartsWith("while")
					? "while"
					: head.Token;

			// 2.	Якщо пріоритет операції, що знаходиться в стеку,
			// 		не менший за пріоритет поточної вхідної операції,
			// 		то операція зі стека подається на вихід і п.2 повторюється,
			if (_operatorsPriorities[headToken] >= _operatorsPriorities[@operator])
			{
				PolishNotation notation = Stack.Pop();
				if (notation.Token != "(") // Причому "(" ніколи не передається на вихід
				{
					ReversePolishNotation.Add(notation); // операція зі стека подається на вихід
				}

				AddOperatorToStack(@operator); // п.2 повторюється
				return;
			}

			// інакше поточна операція заноситься в стек.
			if (@operator != ")") // Причому ")" ніколи не заноситься в стек
			{
				Stack.Push(new PolishNotation {Token = @operator, Type = PolishNotationTokenType.Operator});
			}
		}

		#endregion

		#region Auxilary

		private void MoveNext()
		{
			_i++;
		}

		private string GenerateLabel()
		{
			string label = $"m{_labelNumber++}";
			_labelAddresses.Add(label, _i);
			return label;
		}

		#endregion
	}
}