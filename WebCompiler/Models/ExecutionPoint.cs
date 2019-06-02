using System.Collections.Generic;

namespace WebCompiler.Models
{
	public class ExecutionPoint
	{
		public int PolishNotationIndex { get; set; }

		public Stack<string> Stack { get; set; }

		public List<string> DeclaredIdentifiers { get; set; }

		public Dictionary<string, decimal> IdentifiersValues { get; set; }

		public PolishResult PolishResult { get; set; }
	}
}