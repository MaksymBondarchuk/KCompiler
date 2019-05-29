using System.Collections.Generic;

namespace WebCompiler.Models
{
	public class PolishTrace
	{
		public string Input { get; set; }
		
		public Stack<string> Stack { get; set; }
		
		public List<string> ReversePolishNotation { get; set; }
	}
}