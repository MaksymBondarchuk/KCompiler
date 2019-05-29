using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebCompiler.Models;

namespace WebCompiler.Managers
{
	public class PolishManager : IPolishManager
	{
		public List<PolishNotation> ReversePolishNotation { get; } = new List<PolishNotation>();
		public List<PolishTrace> Trace { get; } = new List<PolishTrace>();

		private int _i;
		private OuterLexemes _outerLexemes;

		private readonly Dictionary<string, int> _operatorsPriorities = new Dictionary<string, int>
		{
			{"do", 1},
			{"while", 1},
			{"enddo", 1},
			{"if", 1},
			{"then", 1},
			{"fi", 1},
			{"read", 1},
			{"write", 1},
			{"(", 2}, // todo: Review me
			{")", 2}, // todo: Review me
			{"var", 1},
			{"set", 1},
			{"equals", 1},
			{"+", 3},
			{"-", 3},
			{"*", 3},
			{"/", 3}
		};

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
//                case "do":
//                    return ParseLoop(lexemes);
//                case "if":
//                    return ParseConditional(lexemes);
				case "read":
					ParseInput();
					break;
				case "write":
					ParseOutput();
					break;
				case "var":
					ParseDeclaration();
					break;
//                case "identifier":
//                    return ParseAssign(lexemes);
			}
		}

		private void ParseDeclaration()
		{
			// "var"
			DijkstraStep("var", PolishNotationTokenType.Operator);

			// Identifier
			DijkstraStep(_outerLexemes.Lexemes[_i].SubString, PolishNotationTokenType.Identifier);

			// "set" (optional)
			if (_outerLexemes.Lexemes[_i].Token.Equals("set"))
			{
				DijkstraStep("set", PolishNotationTokenType.Operator);

				ParseArithmeticExpression();
			}

			DijkstraStep("\\n", PolishNotationTokenType.Delimiter);
		}

		private void ParseInput()
		{
			// "read"
			DijkstraStep("read", PolishNotationTokenType.Operator);

			// "("
			MoveNext(); // Skip

			// Identifier
			DijkstraStep(_outerLexemes.Lexemes[_i].SubString, PolishNotationTokenType.Identifier);

			// ")"
			MoveNext(); // Skip

			DijkstraStep("\\n", PolishNotationTokenType.Delimiter);
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

			DijkstraStep("\\n", PolishNotationTokenType.Delimiter);
		}

		private void ParseArithmeticExpression()
		{
			throw new NotImplementedException();
		}

		#region Stack

		[SuppressMessage("ReSharper", "CommentTypo")]
		private void DijkstraStep(string input, PolishNotationTokenType type)
		{
			switch (type)
			{
				// 1.	Ідентифікатори та константи проходять від входу прямо до виходу
				case PolishNotationTokenType.Identifier:
					ReversePolishNotation.Add(new PolishNotation {Token = input, Type = type});
					break;
				case PolishNotationTokenType.Operator:
					AddOperatorToStack(input);
					break;
				// 4.	Якщо вхідний ланцюжок порожній, всі операції зі стека по черзі передаються на вихід
				// 		за ознакою кінця виразу (наприклад, «;»).
				case PolishNotationTokenType.Delimiter:
					while (Stack.Count != 0)
					{
						ReversePolishNotation.Add(Stack.Pop());
					}

					ReversePolishNotation.Add(new PolishNotation {Token = input, Type = type});

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
			// 3.	Якщо стек порожній, то поточна операція заноситься в стек
			if (Stack.Count == 0)
			{
				Stack.Push(new PolishNotation {Token = @operator, Type = PolishNotationTokenType.Operator});
				return;
			}

			PolishNotation head = Stack.Peek();

			// 2.	Якщо пріоритет операції, що знаходиться в стеку,
			// 		не менший за пріоритет поточної вхідної операції,
			// 		то операція зі стека подається на вихід і п.2 повторюється,
			if (_operatorsPriorities[head.Token] >= _operatorsPriorities[@operator])
			{
				ReversePolishNotation.Add(Stack.Pop()); // операція зі стека подається на вихід
				AddOperatorToStack(@operator); // п.2 повторюється
				return;
			}

			// інакше поточна операція заноситься в стек.
			Stack.Push(new PolishNotation {Token = @operator, Type = PolishNotationTokenType.Operator});
		}

		#endregion

		private void MoveNext()
		{
			_i++;
		}
	}
}