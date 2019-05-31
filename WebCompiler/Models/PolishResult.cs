using System.Collections.Generic;

namespace WebCompiler.Models
{
	public class PolishResult
	{
		public List<PolishNotation> ReversePolishNotation { get; set; }
		public List<PolishTrace> Trace { get; set; }

		public Dictionary<string, int> LabelAddresses { get; set; }
	}
}