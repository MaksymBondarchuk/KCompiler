using System.Collections.Generic;
using WebCompiler.Models;

namespace WebCompiler.Extensions
{
	public static class StackExtensions
	{
		public static Stack<PolishNotation> Clone(this IEnumerable<PolishNotation> stack)
		{
			var clone = new Stack<PolishNotation>();
			foreach (PolishNotation element in stack)
			{
				clone.Push(new PolishNotation
				{
					Token = new string(element.Token),
					Type = element.Type,
					IsAssignmentToThisIdentifier = element.IsAssignmentToThisIdentifier
				});
			}
			return clone;
		}
	}
}