using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using WebCompiler.Exceptions;
using WebCompiler.Models;

namespace WebCompiler.Managers
{
	public class ExecutionManager : IExecutionManager
	{
		private readonly Stack<string> _stack = new Stack<string>();
		private readonly List<string> _declaredIdentifiers = new List<string>();

		private readonly Dictionary<string, decimal> _identifiersValues = new Dictionary<string, decimal>();

		public string Run(PolishResult polishResult)
		{
			var responseBuilder = new StringBuilder();

			_stack.Clear();

			int i = 0;
			while (i < polishResult.ReversePolishNotation.Count)
			{
				PolishNotation element = polishResult.ReversePolishNotation[i];

				switch (element.Type)
				{
					case PolishNotationTokenType.Identifier:
						_stack.Push(_identifiersValues.ContainsKey(element.Token) &&
						            !element.IsAssignmentToThisIdentifier
							? _identifiersValues[element.Token].ToString(CultureInfo.InvariantCulture)
							: element.Token); // only for declaration or assignment
						break;
					case PolishNotationTokenType.Literal:
						_stack.Push(element.Token);
						break;
					case PolishNotationTokenType.Operator:
						switch (element.Token)
						{
							case "@+":
							case "@-":
								HandleArithmeticUnary(element);
								break;
							case "+":
							case "-":
							case "*":
							case "/":
								HandleArithmeticBinary(element);
								break;
							case "var":
								HandleVar();
								break;
							case "set":
								HandleSet();
								break;
							case "write":
								string head = _stack.Pop();
								if (head.StartsWith("@"))
								{
									throw new RuntimeException($"{head} is not declared");
								}

								responseBuilder.AppendLine(head.ToString(CultureInfo.InvariantCulture));
								break;
							case "equals":
							case "greaterthn":
							case "lessthn":
								HandleConditional(element);
								break;
						}
						break;
					case PolishNotationTokenType.Delimiter:
						break;
					case PolishNotationTokenType.If:
						break;
					case PolishNotationTokenType.Then:
						break;
					case PolishNotationTokenType.Fi:
						break;
					case PolishNotationTokenType.While:
						break;
					case PolishNotationTokenType.TechnicalDo:
						break;
					case PolishNotationTokenType.Enddo:
						break;
					case PolishNotationTokenType.Label:
						PolishNotation nextElement = polishResult.ReversePolishNotation[i + 1];
						switch (nextElement.Token)
						{
							case "УПХ":
								bool operand = Convert.ToBoolean(_stack.Pop());
								if (!operand)
								{
									i = polishResult.LabelAddresses[element.Token];
									continue;
								}
								break;
							case "БП":
								i = polishResult.LabelAddresses[element.Token];
								continue;
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				i++;
			}

			return responseBuilder.ToString();
		}

		private void HandleArithmeticUnary(PolishNotation element)
		{
			if (element.Token == "@+")
			{
				return;
			}

			// "-"
			EnsureStackHeadNotIdentifier();
			decimal operand1 = Convert.ToDecimal(_stack.Pop());
			_stack.Push((-operand1).ToString(CultureInfo.InvariantCulture));
		}

		private void HandleArithmeticBinary(PolishNotation element)
		{
			EnsureStackHeadNotIdentifier();
			decimal operand1 = Convert.ToDecimal(_stack.Pop());
			EnsureStackHeadNotIdentifier();
			decimal operand2 = Convert.ToDecimal(_stack.Pop());
			decimal result;

			switch (element.Token)
			{
				case "+":
					result = operand2 + operand1;
					break;
				case "-":
					result = operand2 - operand1;
					break;
				case "*":
					result = operand2 * operand1;
					break;
				case "/":
					if (operand1 == 0)
					{
						throw new RuntimeException("Division by 0");
					}

					result = operand2 / operand1;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(element.Token), element.Token,
						"Unknown binary arithmetic operator");
			}

			_stack.Push(result.ToString(CultureInfo.InvariantCulture));
		}

		private void HandleSet()
		{
			decimal value = Convert.ToDecimal(_stack.Pop());
			string identifier = _stack.Pop();
			if (!_declaredIdentifiers.Contains(identifier))
			{
				throw new RuntimeException($"{identifier} is not declared");
			}

			_identifiersValues[identifier] = value;
		}

		private void HandleVar()
		{
			string identifier = _stack.Peek();
			if (_declaredIdentifiers.Contains(identifier))
			{
				throw new RuntimeException($"{identifier} is already declared");
			}

			_declaredIdentifiers.Add(identifier);
		}

		private void HandleConditional(PolishNotation element)
		{
			EnsureStackHeadNotIdentifier();
			decimal operand1 = Convert.ToDecimal(_stack.Pop());
			EnsureStackHeadNotIdentifier();
			decimal operand2 = Convert.ToDecimal(_stack.Pop());
			bool result;
			
			switch (element.Token)
			{
				case "equals":
					result = decimal.Equals(operand2, operand1);
					break;
				case "greaterthn":
					result = operand2 > operand1;
					break;
				case "lessthn":
					result = operand2 < operand1;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(element.Token), element.Token,
						"Unknown conditional operator");
			}

			_stack.Push(result.ToString());
		}

		private void EnsureStackHeadNotIdentifier()
		{
			string head = _stack.Peek();
			if (head.StartsWith("@") && head != "@-")
			{
				throw new RuntimeException($"Usage of undeclared identifier {head}");
			}
		}
	}
}