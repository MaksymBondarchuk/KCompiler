using System.Collections.Generic;

namespace WebCompiler.Models
{
	public class PolishTrace
	{
		public string Input { get; set; }
		
		public Stack<PolishNotation> Stack { get; set; }
		
		public List<PolishNotation> ReversePolishNotation { get; set; }
	}
}